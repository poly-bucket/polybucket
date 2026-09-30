using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using PolyBucket.Api.Common.Email;
using PolyBucket.Api.Common.Email.Templates;
using PolyBucket.Api.Features.Email.Domain;
using PolyBucket.Api.Features.Email.Repository;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Email;

public class EmailDispatcherTests
{
    private readonly Mock<IEmailOutboxRepository> _repository = new();
    private readonly Mock<IEmailSettingsResolver> _resolver = new();
    private readonly Mock<IEmailTransport> _transport = new();
    private readonly Mock<IEmailTransportFactory> _factory = new();
    private readonly Mock<IEmailBrandingProvider> _branding = new();
    private readonly EmailOptions _options = new();

    public EmailDispatcherTests()
    {
        _resolver.Setup(r => r.GetEffectiveSettingsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EffectiveEmailSettings
            {
                Transport = EmailTransportKind.Log,
                FromAddress = "noreply@example.com",
                PublicBaseUrl = "https://models.example.com"
            });
        _factory.Setup(f => f.Get(EmailTransportKind.Log)).Returns(_transport.Object);
        _branding.Setup(b => b.GetBrandingAsync(It.IsAny<EffectiveEmailSettings>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EmailBranding("PolyBucket", "https://models.example.com"));
    }

    private EmailDispatcher CreateDispatcher() => new(
        _repository.Object,
        _resolver.Object,
        _factory.Object,
        new EmailTemplateRenderer(),
        _branding.Object,
        Options.Create(_options),
        TimeProvider.System,
        NullLogger<EmailDispatcher>.Instance);

    private void SetupBatch(params EmailMessage[] messages)
    {
        _repository.Setup(r => r.ClaimBatchAsync(It.IsAny<int>(), It.IsAny<TimeSpan>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(messages);
    }

    private static EmailMessage ResetMessage(int attempts = 1) => new()
    {
        TemplateKey = EmailTemplateKey.PasswordReset,
        Recipient = "user@example.com",
        Attempts = attempts,
        Status = EmailMessageStatus.Sending,
        ModelJson = JsonSerializer.Serialize(new Dictionary<string, string>
        {
            ["username"] = "maker",
            ["actionUrl"] = "https://models.example.com/reset-password?token=secret",
            ["expiresInMinutes"] = "60"
        })
    };

    [Fact(DisplayName = "When a message is delivered, it is marked sent and its token-bearing model is scrubbed.")]
    public async Task DispatchPending_Success_MarksSentAndScrubs()
    {
        // Arrange
        var message = ResetMessage();
        SetupBatch(message);
        EmailEnvelope? sent = null;
        _transport.Setup(t => t.SendAsync(It.IsAny<EmailEnvelope>(), It.IsAny<EffectiveEmailSettings>(), It.IsAny<CancellationToken>()))
            .Callback<EmailEnvelope, EffectiveEmailSettings, CancellationToken>((e, _, _) => sent = e)
            .Returns(Task.CompletedTask);

        // Act
        var processed = await CreateDispatcher().DispatchPendingAsync();

        // Assert
        processed.ShouldBe(1);
        sent.ShouldNotBeNull();
        sent!.MessageId.ShouldBe(message.Id);
        sent.HtmlBody.ShouldContain("reset-password?token=secret");
        _repository.Verify(r => r.MarkSentAsync(message.Id, It.IsAny<DateTime>(), true, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "When delivery fails with a transient error, the message is rescheduled with a future attempt time.")]
    public async Task DispatchPending_TransientFailure_MarksFailed()
    {
        // Arrange
        var message = ResetMessage(attempts: 1);
        SetupBatch(message);
        _transport.Setup(t => t.SendAsync(It.IsAny<EmailEnvelope>(), It.IsAny<EffectiveEmailSettings>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new EmailDeliveryException("Connection refused", isTransient: true));

        // Act
        await CreateDispatcher().DispatchPendingAsync();

        // Assert
        _repository.Verify(r => r.MarkFailedAsync(
            message.Id,
            "Connection refused",
            It.Is<DateTime>(d => d > DateTime.UtcNow),
            It.IsAny<CancellationToken>()), Times.Once);
        _repository.Verify(r => r.MarkDeadLetterAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "When delivery fails permanently, the message is dead-lettered immediately.")]
    public async Task DispatchPending_PermanentFailure_DeadLetters()
    {
        // Arrange
        var message = ResetMessage(attempts: 1);
        SetupBatch(message);
        _transport.Setup(t => t.SendAsync(It.IsAny<EmailEnvelope>(), It.IsAny<EffectiveEmailSettings>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new EmailDeliveryException("550 mailbox unavailable", isTransient: false));

        // Act
        await CreateDispatcher().DispatchPendingAsync();

        // Assert
        _repository.Verify(r => r.MarkDeadLetterAsync(message.Id, "550 mailbox unavailable", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "When a transient failure happens on the last allowed attempt, the message is dead-lettered.")]
    public async Task DispatchPending_MaxAttemptsReached_DeadLetters()
    {
        // Arrange
        _options.Dispatcher.MaxAttempts = 3;
        var message = ResetMessage(attempts: 3);
        SetupBatch(message);
        _transport.Setup(t => t.SendAsync(It.IsAny<EmailEnvelope>(), It.IsAny<EffectiveEmailSettings>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new EmailDeliveryException("timeout", isTransient: true));

        // Act
        await CreateDispatcher().DispatchPendingAsync();

        // Assert
        _repository.Verify(r => r.MarkDeadLetterAsync(message.Id, "timeout", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "When a message model cannot be parsed, it is dead-lettered without calling the transport.")]
    public async Task DispatchPending_CorruptModel_DeadLetters()
    {
        // Arrange
        var message = ResetMessage();
        message.ModelJson = "not json";
        SetupBatch(message);

        // Act
        await CreateDispatcher().DispatchPendingAsync();

        // Assert
        _repository.Verify(r => r.MarkDeadLetterAsync(message.Id, It.Is<string>(s => s.StartsWith("Render failed")), It.IsAny<CancellationToken>()), Times.Once);
        _transport.Verify(t => t.SendAsync(It.IsAny<EmailEnvelope>(), It.IsAny<EffectiveEmailSettings>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "When email delivery is disabled, nothing is claimed so queued messages wait for a working configuration.")]
    public async Task DispatchPending_EmailDisabled_ClaimsNothing()
    {
        // Arrange
        _resolver.Setup(r => r.GetEffectiveSettingsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EffectiveEmailSettings());

        // Act
        var processed = await CreateDispatcher().DispatchPendingAsync();

        // Assert
        processed.ShouldBe(0);
        _repository.Verify(r => r.ClaimBatchAsync(It.IsAny<int>(), It.IsAny<TimeSpan>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory(DisplayName = "Retry delays grow exponentially, stay within 20% jitter, and cap at six hours.")]
    [InlineData(1, 30)]
    [InlineData(2, 60)]
    [InlineData(4, 240)]
    [InlineData(20, 21600)]
    public void ComputeRetryDelay_GrowsExponentiallyWithCap(int attempts, double expectedSeconds)
    {
        // Arrange
        var lowJitter = -1d;
        var highJitter = 1d;

        // Act
        var low = EmailDispatcher.ComputeRetryDelay(attempts, lowJitter);
        var none = EmailDispatcher.ComputeRetryDelay(attempts, 0);
        var high = EmailDispatcher.ComputeRetryDelay(attempts, highJitter);

        // Assert
        none.TotalSeconds.ShouldBe(expectedSeconds, 0.001);
        low.TotalSeconds.ShouldBe(expectedSeconds * 0.8, 0.001);
        high.TotalSeconds.ShouldBe(expectedSeconds * 1.2, 0.001);
    }

    [Fact(DisplayName = "Notification emails carry a List-Unsubscribe link to notification settings when a public URL is configured.")]
    public void BuildHeaders_NotificationWithPublicUrl_AddsListUnsubscribe()
    {
        // Arrange
        var settings = new EffectiveEmailSettings { Transport = EmailTransportKind.Log, PublicBaseUrl = "https://polybucket.example/" };

        // Act
        var headers = EmailDispatcher.BuildHeaders(EmailTemplateKey.Notification, nameof(EmailTemplateKey.Notification), settings);

        // Assert
        headers["List-Unsubscribe"].ShouldBe("<https://polybucket.example/settings/notifications>");
        headers["Auto-Submitted"].ShouldBe("auto-generated");
    }

    [Theory(DisplayName = "List-Unsubscribe is omitted for account emails and when no public URL is configured.")]
    [InlineData(EmailTemplateKey.PasswordReset, "https://polybucket.example")]
    [InlineData(EmailTemplateKey.Notification, "")]
    public void BuildHeaders_AccountEmailOrNoPublicUrl_OmitsListUnsubscribe(EmailTemplateKey key, string publicBaseUrl)
    {
        // Arrange
        var settings = new EffectiveEmailSettings { Transport = EmailTransportKind.Log, PublicBaseUrl = publicBaseUrl };

        // Act
        var headers = EmailDispatcher.BuildHeaders(key, key.ToString(), settings);

        // Assert
        headers.ContainsKey("List-Unsubscribe").ShouldBeFalse();
        headers["X-PolyBucket-Template"].ShouldBe(key.ToString());
    }
}
