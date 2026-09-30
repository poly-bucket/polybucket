using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Common.Email;
using PolyBucket.Api.Common.Email.Templates;
using PolyBucket.Api.Features.Email.Domain;
using PolyBucket.Api.Features.Notifications.Repository;

namespace PolyBucket.Api.Features.Notifications.Domain;

public class NotificationPublisher(
    INotificationRepository repository,
    IEmailQueue emailQueue,
    IEmailSettingsResolver emailSettingsResolver,
    TimeProvider timeProvider,
    ILogger<NotificationPublisher> logger) : INotificationPublisher
{
    public async Task<bool> PublishAsync(NotificationRequest request, CancellationToken cancellationToken = default)
    {
        if (request.ActorUserId.HasValue && request.ActorUserId.Value == request.RecipientUserId)
        {
            return false;
        }

        var recipient = await repository.GetRecipientAsync(request.RecipientUserId, cancellationToken);
        if (recipient == null || !recipient.Allows(request.Type))
        {
            return false;
        }

        var dedupeKey = Truncate(request.DedupeKey, NotificationLimits.MaxDedupeKeyLength);
        if (dedupeKey != null && await repository.ExistsByDedupeKeyAsync(recipient.UserId, dedupeKey, cancellationToken))
        {
            return false;
        }

        var actorName = await ResolveActorNameAsync(request, cancellationToken);
        var title = Truncate(request.Title.Replace(NotificationRequest.ActorToken, actorName), NotificationLimits.MaxTitleLength)!;
        var message = Truncate(request.Message.Replace(NotificationRequest.ActorToken, actorName), NotificationLimits.MaxMessageLength)!;
        var now = timeProvider.GetUtcNow().UtcDateTime;

        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = recipient.UserId,
            ActorUserId = request.ActorUserId,
            Type = request.Type,
            Priority = request.Priority,
            Title = title,
            Message = message,
            ActionUrl = Truncate(request.ActionUrl, NotificationLimits.MaxActionUrlLength),
            RelatedEntityId = request.RelatedEntityId,
            RelatedEntityType = request.RelatedEntityType,
            DedupeKey = dedupeKey,
            CreatedAt = now,
            CreatedById = request.ActorUserId ?? Guid.Empty,
            UpdatedAt = now,
            UpdatedById = request.ActorUserId ?? Guid.Empty
        };
        repository.Add(notification);

        if (request.SendEmail && recipient.EmailNotifications)
        {
            await QueueEmailAsync(notification, recipient, cancellationToken);
        }

        return true;
    }

    private async Task<string> ResolveActorNameAsync(NotificationRequest request, CancellationToken cancellationToken)
    {
        var needsActor = request.Title.Contains(NotificationRequest.ActorToken, StringComparison.Ordinal)
            || request.Message.Contains(NotificationRequest.ActorToken, StringComparison.Ordinal);
        if (!needsActor)
        {
            return string.Empty;
        }

        var name = request.ActorUserId.HasValue
            ? await repository.GetUsernameAsync(request.ActorUserId.Value, cancellationToken)
            : null;
        return string.IsNullOrWhiteSpace(name) ? "Someone" : name;
    }

    private async Task QueueEmailAsync(Notification notification, NotificationRecipient recipient, CancellationToken cancellationToken)
    {
        var settings = await emailSettingsResolver.GetEffectiveSettingsAsync(cancellationToken);
        if (!settings.IsEnabled)
        {
            return;
        }

        var model = new Dictionary<string, string>
        {
            ["username"] = recipient.Username,
            ["title"] = notification.Title,
            ["message"] = notification.Message
        };
        if (!string.IsNullOrWhiteSpace(notification.ActionUrl) && settings.HasPublicBaseUrl)
        {
            model["actionUrl"] = settings.BuildUrl(notification.ActionUrl);
        }

        try
        {
            await emailQueue.EnqueueAsync(
                new EmailRequest(EmailTemplateKey.Notification, recipient.Email, model, $"notification:{notification.Id}"),
                saveChanges: false,
                cancellationToken);
        }
        catch (ArgumentException ex)
        {
            logger.LogWarning(ex, "Notification email for user {UserId} was not queued", recipient.UserId);
        }
    }

    private static string? Truncate(string? value, int maxLength)
    {
        if (value == null)
        {
            return null;
        }

        return value.Length <= maxLength ? value : value[..maxLength];
    }
}
