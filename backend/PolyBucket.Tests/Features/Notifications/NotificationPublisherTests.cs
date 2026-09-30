using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PolyBucket.Api.Common.Email;
using PolyBucket.Api.Common.Email.Templates;
using PolyBucket.Api.Features.Email.Domain;
using PolyBucket.Api.Features.Notifications.Domain;
using PolyBucket.Api.Features.Notifications.Repository;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Notifications;

public class NotificationPublisherTests
{
    private static readonly Guid RecipientId = Guid.NewGuid();
    private static readonly Guid ActorId = Guid.NewGuid();

    private readonly Mock<INotificationRepository> _repository = new();
    private readonly Mock<IEmailQueue> _emailQueue = new();
    private readonly Mock<IEmailSettingsResolver> _settingsResolver = new();
    private Notification? _added;

    public NotificationPublisherTests()
    {
        _repository.Setup(r => r.Add(It.IsAny<Notification>())).Callback<Notification>(n => _added = n);
        _repository.Setup(r => r.GetUsernameAsync(ActorId, It.IsAny<CancellationToken>())).ReturnsAsync("alice");
        _emailQueue
            .Setup(q => q.EnqueueAsync(It.IsAny<EmailRequest>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(EmailEnqueueOutcome.Queued);
        UseEmailSettings(new EffectiveEmailSettings { Transport = EmailTransportKind.Log, PublicBaseUrl = "https://polybucket.example" });
    }

    private NotificationPublisher CreatePublisher() =>
        new(_repository.Object, _emailQueue.Object, _settingsResolver.Object, TimeProvider.System, NullLogger<NotificationPublisher>.Instance);

    private void UseRecipient(bool emailNotifications = true, bool notifyOnLikes = true, bool notifyOnComments = true)
    {
        _repository
            .Setup(r => r.GetRecipientAsync(RecipientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new NotificationRecipient(RecipientId, "maker", "maker@example.com", emailNotifications, notifyOnLikes, notifyOnComments, true));
    }

    private void UseEmailSettings(EffectiveEmailSettings settings)
    {
        _settingsResolver.Setup(s => s.GetEffectiveSettingsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(settings);
    }

    private static NotificationRequest Request(NotificationType type = NotificationType.ModelApproved, Guid? actorId = null, string? dedupeKey = null, bool sendEmail = true) => new()
    {
        RecipientUserId = RecipientId,
        ActorUserId = actorId ?? ActorId,
        Type = type,
        Title = $"{NotificationRequest.ActorToken} did something",
        Message = $"{NotificationRequest.ActorToken} did something to your model.",
        ActionUrl = "/models/1",
        DedupeKey = dedupeKey,
        SendEmail = sendEmail
    };

    [Fact(DisplayName = "Publishing a notification adds it for the recipient with the actor's name substituted.")]
    public async Task PublishAsync_WithValidRequest_AddsNotificationWithActorName()
    {
        // Arrange
        UseRecipient();
        var publisher = CreatePublisher();

        // Act
        var published = await publisher.PublishAsync(Request());

        // Assert
        published.ShouldBeTrue();
        _added.ShouldNotBeNull();
        _added.UserId.ShouldBe(RecipientId);
        _added.ActorUserId.ShouldBe(ActorId);
        _added.Title.ShouldBe("alice did something");
        _added.Message.ShouldBe("alice did something to your model.");
        _added.IsRead.ShouldBeFalse();
    }

    [Fact(DisplayName = "Publishing falls back to 'Someone' when the actor has no username.")]
    public async Task PublishAsync_WhenActorUnknown_UsesSomeone()
    {
        // Arrange
        UseRecipient();
        var unknownActor = Guid.NewGuid();
        var publisher = CreatePublisher();

        // Act
        await publisher.PublishAsync(Request(actorId: unknownActor));

        // Assert
        _added.ShouldNotBeNull();
        _added.Title.ShouldBe("Someone did something");
    }

    [Fact(DisplayName = "Users are never notified about their own actions.")]
    public async Task PublishAsync_WhenActorIsRecipient_DoesNothing()
    {
        // Arrange
        UseRecipient();
        var publisher = CreatePublisher();

        // Act
        var published = await publisher.PublishAsync(Request(actorId: RecipientId));

        // Assert
        published.ShouldBeFalse();
        _repository.Verify(r => r.Add(It.IsAny<Notification>()), Times.Never);
        _repository.Verify(r => r.GetRecipientAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "Publishing to a missing or deleted recipient does nothing.")]
    public async Task PublishAsync_WhenRecipientMissing_DoesNothing()
    {
        // Arrange
        _repository.Setup(r => r.GetRecipientAsync(RecipientId, It.IsAny<CancellationToken>())).ReturnsAsync((NotificationRecipient?)null);
        var publisher = CreatePublisher();

        // Act
        var published = await publisher.PublishAsync(Request());

        // Assert
        published.ShouldBeFalse();
        _repository.Verify(r => r.Add(It.IsAny<Notification>()), Times.Never);
    }

    [Theory(DisplayName = "Recipients who turned off a category do not receive notifications of that category.")]
    [InlineData(NotificationType.ModelLiked, false, true)]
    [InlineData(NotificationType.CommentLiked, false, true)]
    [InlineData(NotificationType.CommentAdded, true, false)]
    [InlineData(NotificationType.CommentReplied, true, false)]
    public async Task PublishAsync_WhenCategoryDisabled_DoesNothing(NotificationType type, bool notifyOnLikes, bool notifyOnComments)
    {
        // Arrange
        UseRecipient(notifyOnLikes: notifyOnLikes, notifyOnComments: notifyOnComments);
        var publisher = CreatePublisher();

        // Act
        var published = await publisher.PublishAsync(Request(type));

        // Assert
        published.ShouldBeFalse();
        _repository.Verify(r => r.Add(It.IsAny<Notification>()), Times.Never);
    }

    [Fact(DisplayName = "Moderation outcomes are always delivered even when social categories are off.")]
    public async Task PublishAsync_ModerationOutcome_IgnoresSocialPreferences()
    {
        // Arrange
        UseRecipient(notifyOnLikes: false, notifyOnComments: false);
        var publisher = CreatePublisher();

        // Act
        var published = await publisher.PublishAsync(Request(NotificationType.ModelRejected));

        // Assert
        published.ShouldBeTrue();
        _added.ShouldNotBeNull();
    }

    [Fact(DisplayName = "A notification whose dedupe key already exists for the recipient is skipped.")]
    public async Task PublishAsync_WhenDedupeKeyExists_DoesNothing()
    {
        // Arrange
        UseRecipient();
        _repository.Setup(r => r.ExistsByDedupeKeyAsync(RecipientId, "model-like:1:2", It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var publisher = CreatePublisher();

        // Act
        var published = await publisher.PublishAsync(Request(NotificationType.ModelLiked, dedupeKey: "model-like:1:2"));

        // Assert
        published.ShouldBeFalse();
        _repository.Verify(r => r.Add(It.IsAny<Notification>()), Times.Never);
    }

    [Fact(DisplayName = "When the recipient allows email, a notification email joins the caller's transaction with an absolute link.")]
    public async Task PublishAsync_WhenEmailAllowed_QueuesEmailWithoutSaving()
    {
        // Arrange
        UseRecipient(emailNotifications: true);
        EmailRequest? queued = null;
        bool? saveChanges = null;
        _emailQueue
            .Setup(q => q.EnqueueAsync(It.IsAny<EmailRequest>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .Callback<EmailRequest, bool, CancellationToken>((r, s, _) => { queued = r; saveChanges = s; })
            .ReturnsAsync(EmailEnqueueOutcome.Queued);
        var publisher = CreatePublisher();

        // Act
        await publisher.PublishAsync(Request());

        // Assert
        queued.ShouldNotBeNull();
        saveChanges.ShouldBe(false);
        queued.Template.ShouldBe(EmailTemplateKey.Notification);
        queued.Recipient.ShouldBe("maker@example.com");
        queued.IdempotencyKey.ShouldBe($"notification:{_added!.Id}");
        queued.Model["actionUrl"].ShouldBe("https://polybucket.example/models/1");
        queued.Model["title"].ShouldBe("alice did something");
    }

    [Fact(DisplayName = "No email is queued when the recipient turned off email notifications.")]
    public async Task PublishAsync_WhenEmailNotificationsOff_DoesNotQueueEmail()
    {
        // Arrange
        UseRecipient(emailNotifications: false);
        var publisher = CreatePublisher();

        // Act
        var published = await publisher.PublishAsync(Request());

        // Assert
        published.ShouldBeTrue();
        _emailQueue.Verify(q => q.EnqueueAsync(It.IsAny<EmailRequest>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "No email is queued for in-app-only notifications.")]
    public async Task PublishAsync_WhenSendEmailFalse_DoesNotQueueEmail()
    {
        // Arrange
        UseRecipient(emailNotifications: true);
        var publisher = CreatePublisher();

        // Act
        await publisher.PublishAsync(Request(sendEmail: false));

        // Assert
        _emailQueue.Verify(q => q.EnqueueAsync(It.IsAny<EmailRequest>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "No email is queued when email delivery is disabled for the instance.")]
    public async Task PublishAsync_WhenEmailDisabled_DoesNotQueueEmail()
    {
        // Arrange
        UseRecipient(emailNotifications: true);
        UseEmailSettings(new EffectiveEmailSettings());
        var publisher = CreatePublisher();

        // Act
        var published = await publisher.PublishAsync(Request());

        // Assert
        published.ShouldBeTrue();
        _emailQueue.Verify(q => q.EnqueueAsync(It.IsAny<EmailRequest>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "The email omits the action link when no public base URL is configured.")]
    public async Task PublishAsync_WithoutPublicBaseUrl_OmitsActionUrl()
    {
        // Arrange
        UseRecipient(emailNotifications: true);
        UseEmailSettings(new EffectiveEmailSettings { Transport = EmailTransportKind.Log });
        EmailRequest? queued = null;
        _emailQueue
            .Setup(q => q.EnqueueAsync(It.IsAny<EmailRequest>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .Callback<EmailRequest, bool, CancellationToken>((r, _, _) => queued = r)
            .ReturnsAsync(EmailEnqueueOutcome.Queued);
        var publisher = CreatePublisher();

        // Act
        await publisher.PublishAsync(Request());

        // Assert
        queued.ShouldNotBeNull();
        queued.Model.ContainsKey("actionUrl").ShouldBeFalse();
    }

    [Fact(DisplayName = "An invalid recipient address does not prevent the in-app notification.")]
    public async Task PublishAsync_WhenEmailQueueRejectsRecipient_StillAddsNotification()
    {
        // Arrange
        UseRecipient(emailNotifications: true);
        _emailQueue
            .Setup(q => q.EnqueueAsync(It.IsAny<EmailRequest>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentException("bad address"));
        var publisher = CreatePublisher();

        // Act
        var published = await publisher.PublishAsync(Request());

        // Assert
        published.ShouldBeTrue();
        _added.ShouldNotBeNull();
    }

    [Fact(DisplayName = "Overlong titles and messages are truncated to the column limits.")]
    public async Task PublishAsync_WithOverlongText_Truncates()
    {
        // Arrange
        UseRecipient(emailNotifications: false);
        var publisher = CreatePublisher();
        var request = Request() with
        {
            Title = new string('t', NotificationLimits.MaxTitleLength + 50),
            Message = new string('m', NotificationLimits.MaxMessageLength + 50)
        };

        // Act
        await publisher.PublishAsync(request);

        // Assert
        _added.ShouldNotBeNull();
        _added.Title.Length.ShouldBe(NotificationLimits.MaxTitleLength);
        _added.Message.Length.ShouldBe(NotificationLimits.MaxMessageLength);
    }
}
