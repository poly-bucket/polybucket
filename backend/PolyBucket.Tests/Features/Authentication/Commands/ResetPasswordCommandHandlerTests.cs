using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PolyBucket.Api.Common.Http;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Features.Authentication.Domain;
using PolyBucket.Api.Features.Authentication.Repository;
using PolyBucket.Api.Features.Authentication.ResetPassword.Domain;
using PolyBucket.Api.Features.Authentication.Services;
using PolyBucket.Api.Features.Email.Domain;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Authentication.Commands;

public class ResetPasswordCommandHandlerTests
{
    private const string RawToken = "raw-reset-token";

    private readonly Mock<IAuthenticationRepository> _repository = new();
    private readonly Mock<IPasswordHasher> _hasher = new();
    private readonly Mock<IAccountEmailService> _accountEmail = new();

    public ResetPasswordCommandHandlerTests()
    {
        _hasher.Setup(h => h.GenerateSalt()).Returns("salt");
        _hasher.Setup(h => h.HashPassword(It.IsAny<string>(), "salt")).Returns("new-hash");
    }

    private ResetPasswordCommandHandler CreateHandler() =>
        new(_repository.Object, _hasher.Object, _accountEmail.Object, TimeProvider.System, NullLogger<ResetPasswordCommandHandler>.Instance);

    private static ResetPasswordCommand Command(string token = RawToken) => new()
    {
        Token = token,
        NewPassword = "NewPassword1!",
        ConfirmPassword = "NewPassword1!",
        Client = new ClientRequestInfo("203.0.113.9", "tests")
    };

    private (PasswordResetToken Token, User User) SetupValidToken(bool tokenConsumed = true)
    {
        var token = new PasswordResetToken
        {
            Id = Guid.NewGuid(),
            Token = TokenHasher.Hash(RawToken),
            Email = "user@example.com",
            ExpiresAt = DateTime.UtcNow.AddMinutes(30),
            CreatedAt = DateTime.UtcNow
        };
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = token.Email,
            Username = "user",
            PasswordHash = "old-hash",
            RequiresPasswordChange = true
        };
        _repository.Setup(r => r.GetPasswordResetTokenByHashAsync(TokenHasher.Hash(RawToken), It.IsAny<CancellationToken>())).ReturnsAsync(token);
        _repository.Setup(r => r.GetUserForUpdateByEmailAsync(token.Email, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _repository.Setup(r => r.TryConsumePasswordResetTokenAsync(token.Id, It.IsAny<DateTime>(), It.IsAny<CancellationToken>())).ReturnsAsync(tokenConsumed);
        return (token, user);
    }

    [Fact(DisplayName = "When the reset token is valid, the handler sets the new password, verifies the email, and revokes every session.")]
    public async Task Handle_ValidToken_ShouldResetPasswordAndRevokeSessions()
    {
        // Arrange
        var (_, user) = SetupValidToken();

        // Act
        await CreateHandler().Handle(Command(), CancellationToken.None);

        // Assert
        user.PasswordHash.ShouldBe("new-hash");
        user.Salt.ShouldBe("salt");
        user.RequiresPasswordChange.ShouldBeFalse();
        user.EmailVerifiedAt.ShouldNotBeNull();
        _repository.Verify(r => r.RevokeAllRefreshTokensForUserAsync(user.Id, "Password reset", "203.0.113.9"), Times.Once);
        _repository.Verify(r => r.InvalidateOutstandingPasswordResetTokensAsync(user.Email, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
        _accountEmail.Verify(a => a.SendPasswordChangedAsync(user, It.Is<ClientRequestInfo>(c => c.IpAddress == "203.0.113.9"), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "When the reset token was already consumed by another request, the password is not changed.")]
    public async Task Handle_TokenAlreadyConsumed_ShouldThrowAndKeepPassword()
    {
        // Arrange
        var (_, user) = SetupValidToken(tokenConsumed: false);

        // Act
        var act = () => CreateHandler().Handle(Command(), CancellationToken.None);

        // Assert
        await Should.ThrowAsync<InvalidOperationException>(act);
        user.PasswordHash.ShouldBe("old-hash");
        _repository.Verify(r => r.RevokeAllRefreshTokensForUserAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact(DisplayName = "When the reset token has expired, the handler rejects it before loading the user.")]
    public async Task Handle_ExpiredToken_ShouldThrow()
    {
        // Arrange
        _repository.Setup(r => r.GetPasswordResetTokenByHashAsync(TokenHasher.Hash(RawToken), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PasswordResetToken
            {
                Id = Guid.NewGuid(),
                Token = TokenHasher.Hash(RawToken),
                Email = "user@example.com",
                ExpiresAt = DateTime.UtcNow.AddMinutes(-5)
            });

        // Act
        var act = () => CreateHandler().Handle(Command(), CancellationToken.None);

        // Assert
        var ex = await Should.ThrowAsync<InvalidOperationException>(act);
        ex.Message.ShouldBe(ResetPasswordCommandHandler.InvalidTokenMessage);
        _repository.Verify(r => r.GetUserForUpdateByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "When the raw token is looked up, only its hash is sent to the repository.")]
    public async Task Handle_ShouldLookUpTokenByHashOnly()
    {
        // Arrange
        var handler = CreateHandler();

        // Act
        await Should.ThrowAsync<InvalidOperationException>(() => handler.Handle(Command("some-raw-token"), CancellationToken.None));

        // Assert
        _repository.Verify(r => r.GetPasswordResetTokenByHashAsync(TokenHasher.Hash("some-raw-token"), It.IsAny<CancellationToken>()), Times.Once);
        _repository.Verify(r => r.GetPasswordResetTokenByHashAsync("some-raw-token", It.IsAny<CancellationToken>()), Times.Never);
    }
}
