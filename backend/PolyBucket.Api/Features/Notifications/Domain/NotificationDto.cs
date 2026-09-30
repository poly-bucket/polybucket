using System;

namespace PolyBucket.Api.Features.Notifications.Domain;

public class NotificationDto
{
    public Guid Id { get; set; }
    public NotificationType Type { get; set; }
    public NotificationPriority Priority { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? ActionUrl { get; set; }
    public Guid? RelatedEntityId { get; set; }
    public string? RelatedEntityType { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public DateTime CreatedAt { get; set; }

    public static NotificationDto From(Notification notification) => new()
    {
        Id = notification.Id,
        Type = notification.Type,
        Priority = notification.Priority,
        Title = notification.Title,
        Message = notification.Message,
        ActionUrl = notification.ActionUrl,
        RelatedEntityId = notification.RelatedEntityId,
        RelatedEntityType = notification.RelatedEntityType,
        IsRead = notification.IsRead,
        ReadAt = notification.ReadAt,
        CreatedAt = notification.CreatedAt
    };
}
