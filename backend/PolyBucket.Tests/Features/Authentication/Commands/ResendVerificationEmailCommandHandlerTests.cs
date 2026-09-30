using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PolyBucket.Api.Common.Email;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Features.Authentication.Domain;
using PolyBucket.Api.Features.Authentication.Repository;
using PolyBucket.Api.Features.Authentication.ResendVerificationEmail.Domain;
using PolyBucket.Api.Features.Authentication.Services;
using PolyBucket.Api.Features.Email.Domain;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Authentication.Commands;

public class ResendVerificationEmailCommandHandlerTests
{
    private readonly Mock<IAuthenticationRepository> _repository = new();
    private readonly Mock<ITokenService> _tokenService = new();
    private readonly Mock<IEmailSettingsResolver> _resolver = new();
    private readonly Mock<IAccountEmailService> _accountEmail = new();

    public ResendVerificationEmailCommandHandlerTests()
    {
        _tokenService.Setup(t => t.GenerateEmailVerificationToken()).Returns("fresh-raw-token");
        SetupEmail(canDeliver: true);
    }

    private void SetupEmail(bool canDeliver)
    {
        _resolver.Setup(r => r.GetEffectiveSettingsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(canDeliver
                ? new EffectiveEmailSettings { Transport = EmailTransportKind.Log, FromAddress = "noreply@example.com", PublicBaseUrl = "https://models.example.com" }
                : new EffectiveEmailSettings());
    }

    private ResendVerificationEmailCommandHandler CreateHandler() =>
        new(_repository.Object, _tokenService.Object, _resolver.Object, _accountEmail.Object, TimeProvider.System, NullLogger<ResendVerificationEmailCommandHandler>.Instance);

    [Fact(DisplayName = "When an unverified user asks for a new link, previous links are invalidated and a hashed token is stored.")]
    public async Task Handle_UnverifiedUser_ShouldInvalidatePreviousAndSendNewLink()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid(), Email = "user@example.com", Username = "user" };
        _repository.Setup(r => r.GetUserByEmailAsync("user@example.com")).ReturnsAsync(user);

        // Act
        var outcome = await CreateHandler().Handle(new ResendVerificationEmailCommand { Email = "user@example.com" }, CancellationToken.None);

        // Assert
        outcome.ShouldBe(ResendVerificationOutcome.Sent);
        _repository.Verify(r => r.InvalidateOutstandingEmailVerificationTokensAsync(user.Email, EmailVerificationPurpose.VerifyAddress, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
        _repository.Verify(r => r.CreateEmailVerificationTokenAsync(It.Is<EmailVerificationToken>(t =>
            t.Token == TokenHasher.Hash("fresh-raw-token") && t.UserId == user.Id)), Times.Once);
        _accountEmail.Verify(a => a.SendVerificationAsync(user, "fresh-raw-token", It.IsAny<TimeSpan>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "When a link was sent less than a minute ago, the resend is throttled and nothing is sent.")]
    public async Task Handle_RecentlySent_ShouldThrottle()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid(), Email = "user@example.com", Username = "user" };
        _repository.Setup(r => r.GetUserByEmailAsync("user@example.com")).ReturnsAsync(user);
        _repository.Setup(r => r.GetLatestEmailVerificationTokenCreatedAtAsync(user.Email, EmailVerificationPurpose.VerifyAddress, It.IsAny<CancellationToken>()))
            .ReturnsAsync(DateTime.UtcNow.AddSeconds(-10));

        // Act
        var outcome = await CreateHandler().Handle(new ResendVerificationEmailCommand { Email = "user@example.com" }, CancellationToken.None);

        // Assert
        outcome.ShouldBe(ResendVerificationOutcome.Throttled);
        _accountEmail.VerifyNoOtherCalls();
        _repository.Verify(r => r.CreateEmailVerificationTokenAsync(It.IsAny<EmailVerificationToken>()), Times.Never);
    }

    [Fact(DisplayName = "When the address is unknown or already verified, nothing is sent.")]
    public async Task Handle_VerifiedUser_ShouldSendNothing()
    {
        // Arrange
        _repository.Setup(r => r.GetUserByEmailAsync("user@example.com"))
            .ReturnsAsync(new User { Id = Guid.NewGuid(), Email = "user@example.com", EmailVerifiedAt = DateTime.UtcNow });

        // Act
        var outcome = await CreateHandler().Handle(new ResendVerificationEmailCommand { Email = "user@example.com" }, CancellationToken.None);

        // Assert
        outcome.ShouldBe(ResendVerificationOutcome.UnknownOrVerified);
        _accountEmail.VerifyNoOtherCalls();
    }

    [Fact(DisplayName = "When email delivery is not configured, the resend does nothing.")]
    public async Task Handle_EmailDisabled_ShouldSendNothing()
    {
        // Arrange
        SetupEmail(canDeliver: false);

        // Act
        var outcome = await CreateHandler().Handle(new ResendVerificationEmailCommand { Email = "user@example.com" }, CancellationToken.None);

        // Assert
        outcome.ShouldBe(ResendVerificationOutcome.EmailUnavailable);
        _repository.Verify(r => r.GetUserByEmailAsync(It.IsAny<string>()), Times.Never);
    }
}
