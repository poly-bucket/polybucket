using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PolyBucket.Api.Common.Email;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Features.Authentication.Domain;
using PolyBucket.Api.Features.Authentication.Repository;
using PolyBucket.Api.Features.Authentication.RequestEmailChange.Domain;
using PolyBucket.Api.Features.Authentication.Services;
using PolyBucket.Api.Features.Email.Domain;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Authentication.Commands;

public class RequestEmailChangeCommandHandlerTests
{
    private readonly Mock<IAuthenticationRepository> _repository = new();
    private readonly Mock<ITokenService> _tokenService = new();
    private readonly Mock<IPasswordHasher> _hasher = new();
    private readonly Mock<IEmailSettingsResolver> _resolver = new();
    private readonly Mock<IAccountEmailService> _accountEmail = new();
    private readonly User _user = new() { Id = Guid.NewGuid(), Email = "old@example.com", Username = "user", PasswordHash = "hash" };

    public RequestEmailChangeCommandHandlerTests()
    {
        _tokenService.Setup(t => t.GenerateEmailVerificationToken()).Returns("change-raw-token");
        _hasher.Setup(h => h.VerifyPassword("correct", "hash")).Returns(true);
        _repository.Setup(r => r.GetUserForUpdateByIdAsync(_user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(_user);
        SetupEmail(canDeliver: true);
    }

    private void SetupEmail(bool canDeliver)
    {
        _resolver.Setup(r => r.GetEffectiveSettingsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(canDeliver
                ? new EffectiveEmailSettings { Transport = EmailTransportKind.Log, FromAddress = "noreply@example.com", PublicBaseUrl = "https://models.example.com" }
                : new EffectiveEmailSettings());
    }

    private RequestEmailChangeCommandHandler CreateHandler() =>
        new(_repository.Object, _tokenService.Object, _hasher.Object, _resolver.Object, _accountEmail.Object, TimeProvider.System, NullLogger<RequestEmailChangeCommandHandler>.Instance);

    private RequestEmailChangeCommand Command(string newEmail = "new@example.com", string password = "correct") => new()
    {
        NewEmail = newEmail,
        CurrentPassword = password,
        UserId = _user.Id
    };

    [Fact(DisplayName = "When the password is correct, the new address is stored as pending and a confirmation link is sent to it.")]
    public async Task Handle_Valid_ShouldStorePendingEmailAndSendConfirmation()
    {
        // Arrange
        var handler = CreateHandler();

        // Act
        await handler.Handle(Command(), CancellationToken.None);

        // Assert
        _user.PendingEmail.ShouldBe("new@example.com");
        _user.Email.ShouldBe("old@example.com");
        _repository.Verify(r => r.CreateEmailVerificationTokenAsync(It.Is<EmailVerificationToken>(t =>
            t.Email == "new@example.com"
            && t.UserId == _user.Id
            && t.Purpose == EmailVerificationPurpose.ChangeAddress
            && t.Token == TokenHasher.Hash("change-raw-token"))), Times.Once);
        _accountEmail.Verify(a => a.SendEmailChangeRequestedAsync(_user, "new@example.com", "change-raw-token", It.IsAny<TimeSpan>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "When the current password is wrong, the email change is refused.")]
    public async Task Handle_WrongPassword_ShouldThrowUnauthorized()
    {
        // Arrange
        var handler = CreateHandler();

        // Act
        var act = () => handler.Handle(Command(password: "wrong"), CancellationToken.None);

        // Assert
        await Should.ThrowAsync<UnauthorizedAccessException>(act);
        _user.PendingEmail.ShouldBeNull();
    }

    [Fact(DisplayName = "When the new address belongs to another account, the email change is refused.")]
    public async Task Handle_EmailTaken_ShouldThrow()
    {
        // Arrange
        _repository.Setup(r => r.IsEmailTakenAsync("new@example.com")).ReturnsAsync(true);

        // Act
        var act = () => CreateHandler().Handle(Command(), CancellationToken.None);

        // Assert
        var ex = await Should.ThrowAsync<InvalidOperationException>(act);
        ex.Message.ShouldBe(RequestEmailChangeCommandHandler.EmailInUseMessage);
    }

    [Fact(DisplayName = "When email delivery is not configured, users cannot change their own address.")]
    public async Task Handle_EmailUnavailable_ShouldThrow()
    {
        // Arrange
        SetupEmail(canDeliver: false);

        // Act
        var act = () => CreateHandler().Handle(Command(), CancellationToken.None);

        // Assert
        var ex = await Should.ThrowAsync<InvalidOperationException>(act);
        ex.Message.ShouldBe(RequestEmailChangeCommandHandler.EmailUnavailableMessage);
    }
}
