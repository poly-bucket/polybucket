using System;

namespace PolyBucket.Api.Features.Notifications.Domain;

public sealed record NotificationRequest
{
    public const string ActorToken = "{actor}";

    public required Guid RecipientUserId { get; init; }
    public Guid? ActorUserId { get; init; }
    public required NotificationType Type { get; init; }
    public required string Title { get; init; }
    public required string Message { get; init; }
    public string? ActionUrl { get; init; }
    public Guid? RelatedEntityId { get; init; }
    public string? RelatedEntityType { get; init; }
    public NotificationPriority Priority { get; init; } = NotificationPriority.Normal;
    public string? DedupeKey { get; init; }
    public bool SendEmail { get; init; } = true;
}
