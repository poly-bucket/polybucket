using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Features.Authentication.Domain;
using PolyBucket.Api.Features.Authentication.Repository;
using PolyBucket.Api.Features.Authentication.Services;
using PolyBucket.Api.Features.Authentication.VerifyEmail.Domain;
using PolyBucket.Api.Features.Email.Domain;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Authentication.Commands;

public class VerifyEmailCommandHandlerTests
{
    private const string RawToken = "raw-verification-token";

    private readonly Mock<IAuthenticationRepository> _repository = new();
    private readonly Mock<IAccountEmailService> _accountEmail = new();

    private VerifyEmailCommandHandler CreateHandler() =>
        new(_repository.Object, _accountEmail.Object, TimeProvider.System, NullLogger<VerifyEmailCommandHandler>.Instance);

    private EmailVerificationToken SetupToken(
        string email = "user@example.com",
        EmailVerificationPurpose purpose = EmailVerificationPurpose.VerifyAddress,
        Guid? userId = null,
        bool isUsed = false,
        DateTime? expiresAt = null)
    {
        var token = new EmailVerificationToken
        {
            Id = Guid.NewGuid(),
            Token = TokenHasher.Hash(RawToken),
            Email = email,
            UserId = userId,
            Purpose = purpose,
            IsUsed = isUsed,
            ExpiresAt = expiresAt ?? DateTime.UtcNow.AddHours(1),
            CreatedAt = DateTime.UtcNow
        };
        _repository
            .Setup(r => r.GetEmailVerificationTokenByHashAsync(TokenHasher.Hash(RawToken), It.IsAny<CancellationToken>()))
            .ReturnsAsync(token);
        return token;
    }

    [Fact(DisplayName = "When the token is valid, the handler looks it up by hash, consumes it once, and marks the user verified.")]
    public async Task Handle_ValidToken_ShouldVerifyUser()
    {
        // Arrange
        var token = SetupToken();
        var user = new User { Id = Guid.NewGuid(), Email = "user@example.com", Username = "user" };
        _repository.Setup(r => r.GetUserForUpdateByEmailAsync(token.Email, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _repository.Setup(r => r.TryConsumeEmailVerificationTokenAsync(token.Id, It.IsAny<DateTime>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

        // Act
        var result = await CreateHandler().Handle(new VerifyEmailCommand { Token = RawToken }, CancellationToken.None);

        // Assert
        result.Purpose.ShouldBe(EmailVerificationPurpose.VerifyAddress);
        user.EmailVerifiedAt.ShouldNotBeNull();
        _repository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _accountEmail.VerifyNoOtherCalls();
    }

    [Fact(DisplayName = "When the token does not exist, the handler rejects it without touching any user.")]
    public async Task Handle_UnknownToken_ShouldThrow()
    {
        // Arrange
        var handler = CreateHandler();

        // Act
        var act = () => handler.Handle(new VerifyEmailCommand { Token = "unknown" }, CancellationToken.None);

        // Assert
        var ex = await Should.ThrowAsync<InvalidOperationException>(act);
        ex.Message.ShouldBe(VerifyEmailCommandHandler.InvalidTokenMessage);
        _repository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "When the token has expired, the handler rejects it.")]
    public async Task Handle_ExpiredToken_ShouldThrow()
    {
        // Arrange
        SetupToken(expiresAt: DateTime.UtcNow.AddMinutes(-1));

        // Act
        var act = () => CreateHandler().Handle(new VerifyEmailCommand { Token = RawToken }, CancellationToken.None);

        // Assert
        await Should.ThrowAsync<InvalidOperationException>(act);
        _repository.Verify(r => r.TryConsumeEmailVerificationTokenAsync(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "When a concurrent request already consumed the token, the handler rejects the second use.")]
    public async Task Handle_TokenConsumedConcurrently_ShouldThrow()
    {
        // Arrange
        var token = SetupToken();
        var user = new User { Id = Guid.NewGuid(), Email = token.Email, Username = "user" };
        _repository.Setup(r => r.GetUserForUpdateByEmailAsync(token.Email, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _repository.Setup(r => r.TryConsumeEmailVerificationTokenAsync(token.Id, It.IsAny<DateTime>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

        // Act
        var act = () => CreateHandler().Handle(new VerifyEmailCommand { Token = RawToken }, CancellationToken.None);

        // Assert
        await Should.ThrowAsync<InvalidOperationException>(act);
        user.EmailVerifiedAt.ShouldBeNull();
    }

    [Fact(DisplayName = "When an email is supplied that does not match the token, the handler rejects it.")]
    public async Task Handle_MismatchedEmail_ShouldThrow()
    {
        // Arrange
        SetupToken(email: "user@example.com");

        // Act
        var act = () => CreateHandler().Handle(new VerifyEmailCommand { Token = RawToken, Email = "other@example.com" }, CancellationToken.None);

        // Assert
        await Should.ThrowAsync<InvalidOperationException>(act);
    }

    [Fact(DisplayName = "When an email-change token is confirmed, the handler swaps the address and notifies the old one.")]
    public async Task Handle_EmailChangeToken_ShouldSwapEmailAndNotifyPreviousAddress()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var token = SetupToken(email: "new@example.com", purpose: EmailVerificationPurpose.ChangeAddress, userId: userId);
        var user = new User { Id = userId, Email = "old@example.com", PendingEmail = "new@example.com", Username = "user" };
        _repository.Setup(r => r.GetUserForUpdateByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _repository.Setup(r => r.IsEmailTakenAsync("new@example.com")).ReturnsAsync(false);
        _repository.Setup(r => r.TryConsumeEmailVerificationTokenAsync(token.Id, It.IsAny<DateTime>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

        // Act
        var result = await CreateHandler().Handle(new VerifyEmailCommand { Token = RawToken }, CancellationToken.None);

        // Assert
        result.Purpose.ShouldBe(EmailVerificationPurpose.ChangeAddress);
        user.Email.ShouldBe("new@example.com");
        user.PendingEmail.ShouldBeNull();
        user.EmailVerifiedAt.ShouldNotBeNull();
        _accountEmail.Verify(a => a.SendEmailChangedNoticeAsync(user, "old@example.com", "new@example.com", It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "When the pending email was replaced by a newer request, the older email-change link is rejected.")]
    public async Task Handle_EmailChangeTokenForStalePendingEmail_ShouldThrow()
    {
        // Arrange
        var userId = Guid.NewGuid();
        SetupToken(email: "first@example.com", purpose: EmailVerificationPurpose.ChangeAddress, userId: userId);
        var user = new User { Id = userId, Email = "old@example.com", PendingEmail = "second@example.com", Username = "user" };
        _repository.Setup(r => r.GetUserForUpdateByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        // Act
        var act = () => CreateHandler().Handle(new VerifyEmailCommand { Token = RawToken }, CancellationToken.None);

        // Assert
        await Should.ThrowAsync<InvalidOperationException>(act);
        user.Email.ShouldBe("old@example.com");
    }

    [Fact(DisplayName = "When the new address was claimed by another account in the meantime, the email change is rejected.")]
    public async Task Handle_EmailChangeToAddressNowTaken_ShouldThrow()
    {
        // Arrange
        var userId = Guid.NewGuid();
        SetupToken(email: "new@example.com", purpose: EmailVerificationPurpose.ChangeAddress, userId: userId);
        var user = new User { Id = userId, Email = "old@example.com", PendingEmail = "new@example.com", Username = "user" };
        _repository.Setup(r => r.GetUserForUpdateByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _repository.Setup(r => r.IsEmailTakenAsync("new@example.com")).ReturnsAsync(true);

        // Act
        var act = () => CreateHandler().Handle(new VerifyEmailCommand { Token = RawToken }, CancellationToken.None);

        // Assert
        var ex = await Should.ThrowAsync<InvalidOperationException>(act);
        ex.Message.ShouldBe(VerifyEmailCommandHandler.EmailInUseMessage);
    }
}
