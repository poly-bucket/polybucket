using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PolyBucket.Api.Common.Email;
using PolyBucket.Api.Common.Email.Templates;
using PolyBucket.Api.Features.Email.Domain;
using PolyBucket.Api.Features.Email.Repository;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Email;

public class EmailQueueTests
{
    private readonly Mock<IEmailOutboxRepository> _repository = new();
    private readonly Mock<IEmailSettingsResolver> _resolver = new();
    private readonly Mock<IEmailDispatchSignal> _signal = new();

    private EmailQueue CreateQueue(bool enabled = true)
    {
        _resolver.Setup(r => r.GetEffectiveSettingsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(enabled
                ? new EffectiveEmailSettings { Transport = EmailTransportKind.Log, FromAddress = "noreply@example.com" }
                : new EffectiveEmailSettings());
        return new EmailQueue(_repository.Object, _resolver.Object, _signal.Object, NullLogger<EmailQueue>.Instance);
    }

    private static EmailRequest WelcomeRequest(string? idempotencyKey = null) =>
        new(EmailTemplateKey.Welcome, "user@example.com", new Dictionary<string, string> { ["username"] = "maker" }, idempotencyKey);

    [Fact(DisplayName = "When email is enabled, a valid request is stored as a pending message and the dispatcher is signalled.")]
    public async Task Enqueue_ValidRequest_AddsPendingMessage()
    {
        // Arrange
        var queue = CreateQueue();
        EmailMessage? added = null;
        _repository.Setup(r => r.Add(It.IsAny<EmailMessage>())).Callback<EmailMessage>(m => added = m);

        // Act
        var outcome = await queue.EnqueueAsync(WelcomeRequest());

        // Assert
        outcome.ShouldBe(EmailEnqueueOutcome.Queued);
        added.ShouldNotBeNull();
        added!.Status.ShouldBe(EmailMessageStatus.Pending);
        added.ModelJson.ShouldContain("maker");
        _repository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _signal.Verify(s => s.Notify(), Times.Once);
    }

    [Fact(DisplayName = "When saveChanges is false, the message is added to the unit of work without saving so it commits with the caller.")]
    public async Task Enqueue_WithoutSave_DoesNotSave()
    {
        // Arrange
        var queue = CreateQueue();

        // Act
        await queue.EnqueueAsync(WelcomeRequest(), saveChanges: false);

        // Assert
        _repository.Verify(r => r.Add(It.IsAny<EmailMessage>()), Times.Once);
        _repository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "When email delivery is disabled, nothing is stored.")]
    public async Task Enqueue_EmailDisabled_ReturnsDisabled()
    {
        // Arrange
        var queue = CreateQueue(enabled: false);

        // Act
        var outcome = await queue.EnqueueAsync(WelcomeRequest());

        // Assert
        outcome.ShouldBe(EmailEnqueueOutcome.EmailDisabled);
        _repository.Verify(r => r.Add(It.IsAny<EmailMessage>()), Times.Never);
    }

    [Fact(DisplayName = "When the idempotency key was already used, the request is reported as a duplicate and not stored.")]
    public async Task Enqueue_DuplicateIdempotencyKey_ReturnsDuplicate()
    {
        // Arrange
        var queue = CreateQueue();
        _repository.Setup(r => r.ExistsByIdempotencyKeyAsync("welcome:1", It.IsAny<CancellationToken>())).ReturnsAsync(true);

        // Act
        var outcome = await queue.EnqueueAsync(WelcomeRequest("welcome:1"));

        // Assert
        outcome.ShouldBe(EmailEnqueueOutcome.Duplicate);
        _repository.Verify(r => r.Add(It.IsAny<EmailMessage>()), Times.Never);
    }

    [Fact(DisplayName = "When a required template field is missing, enqueueing throws before anything is stored.")]
    public async Task Enqueue_MissingRequiredField_Throws()
    {
        // Arrange
        var queue = CreateQueue();
        var request = new EmailRequest(EmailTemplateKey.PasswordReset, "user@example.com", new Dictionary<string, string> { ["username"] = "maker" });

        // Act
        var exception = await Should.ThrowAsync<ArgumentException>(() => queue.EnqueueAsync(request));

        // Assert
        exception.Message.ShouldContain("actionUrl");
        _repository.Verify(r => r.Add(It.IsAny<EmailMessage>()), Times.Never);
    }

    [Fact(DisplayName = "When the recipient is not a valid address, enqueueing throws.")]
    public async Task Enqueue_InvalidRecipient_Throws()
    {
        // Arrange
        var queue = CreateQueue();
        var request = new EmailRequest(EmailTemplateKey.Welcome, "not-an-email", new Dictionary<string, string> { ["username"] = "maker" });

        // Act
        var act = () => queue.EnqueueAsync(request);

        // Assert
        await Should.ThrowAsync<ArgumentException>(act);
    }
}
