using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PolyBucket.Api.Common.Email;
using PolyBucket.Api.Common.Email.Templates;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Features.Email.Domain;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Email;

public class AccountEmailServiceTests
{
    private readonly Mock<IEmailQueue> _queue = new();
    private readonly Mock<IEmailSettingsResolver> _resolver = new();
    private readonly User _user = new() { Id = Guid.NewGuid(), Email = "user@example.com", Username = "maker" };

    private AccountEmailService CreateService(string publicBaseUrl = "https://models.example.com/")
    {
        _resolver.Setup(r => r.GetEffectiveSettingsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EffectiveEmailSettings
            {
                Transport = EmailTransportKind.Log,
                FromAddress = "noreply@example.com",
                PublicBaseUrl = publicBaseUrl
            });
        _queue.Setup(q => q.EnqueueAsync(It.IsAny<EmailRequest>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(EmailEnqueueOutcome.Queued);
        return new AccountEmailService(_queue.Object, _resolver.Object, TimeProvider.System, NullLogger<AccountEmailService>.Instance);
    }

    [Fact(DisplayName = "When a password reset is sent, the link uses the public base URL and an escaped token.")]
    public async Task SendPasswordReset_BuildsAbsoluteLink()
    {
        // Arrange
        var service = CreateService();
        EmailRequest? captured = null;
        _queue.Setup(q => q.EnqueueAsync(It.IsAny<EmailRequest>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .Callback<EmailRequest, bool, CancellationToken>((r, _, _) => captured = r)
            .ReturnsAsync(EmailEnqueueOutcome.Queued);

        // Act
        var outcome = await service.SendPasswordResetAsync(_user, "a+b/c", TimeSpan.FromHours(1));

        // Assert
        outcome.ShouldBe(EmailEnqueueOutcome.Queued);
        captured.ShouldNotBeNull();
        captured!.Template.ShouldBe(EmailTemplateKey.PasswordReset);
        captured.Model["actionUrl"].ShouldBe("https://models.example.com/reset-password?token=a%2Bb%2Fc");
        captured.Model["expiresInMinutes"].ShouldBe("60");
    }

    [Fact(DisplayName = "When no public base URL is configured, link emails are not queued.")]
    public async Task SendVerification_WithoutPublicBaseUrl_ReturnsMissingPublicBaseUrl()
    {
        // Arrange
        var service = CreateService(publicBaseUrl: string.Empty);

        // Act
        var outcome = await service.SendVerificationAsync(_user, "token", TimeSpan.FromHours(24));

        // Assert
        outcome.ShouldBe(EmailEnqueueOutcome.MissingPublicBaseUrl);
        _queue.Verify(q => q.EnqueueAsync(It.IsAny<EmailRequest>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "When an email change completes, the notice goes to the previous address with the new address masked.")]
    public async Task SendEmailChangedNotice_MasksNewAddress()
    {
        // Arrange
        var service = CreateService();
        EmailRequest? captured = null;
        _queue.Setup(q => q.EnqueueAsync(It.IsAny<EmailRequest>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .Callback<EmailRequest, bool, CancellationToken>((r, _, _) => captured = r)
            .ReturnsAsync(EmailEnqueueOutcome.Queued);

        // Act
        await service.SendEmailChangedNoticeAsync(_user, "old@example.com", "newaddress@example.com");

        // Assert
        captured!.Recipient.ShouldBe("old@example.com");
        captured.Model["newEmail"].ShouldBe("n***@example.com");
    }

    [Fact(DisplayName = "When a welcome email is sent, it uses a per-user idempotency key so retries don't send duplicates.")]
    public async Task SendWelcome_UsesIdempotencyKey()
    {
        // Arrange
        var service = CreateService();
        EmailRequest? captured = null;
        _queue.Setup(q => q.EnqueueAsync(It.IsAny<EmailRequest>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .Callback<EmailRequest, bool, CancellationToken>((r, _, _) => captured = r)
            .ReturnsAsync(EmailEnqueueOutcome.Queued);

        // Act
        await service.SendWelcomeAsync(_user);

        // Assert
        captured!.IdempotencyKey.ShouldBe($"welcome:{_user.Id}");
    }
}
