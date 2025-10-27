using PolyBucket.Api.Common.Entities;
using System;

namespace PolyBucket.Api.Features.Notifications.Domain
{
    public class Notification : Auditable
    {
        public Guid UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public NotificationType Type { get; set; }
        public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;
        public bool IsRead { get; set; } = false;
        public DateTime? ReadAt { get; set; }
        public string? ActionUrl { get; set; }
        public string? IconUrl { get; set; }
        public Guid? RelatedEntityId { get; set; }
        public string? RelatedEntityType { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public bool IsSystemNotification { get; set; } = false;
        public string? Metadata { get; set; } // JSON for additional data
    }

    public enum NotificationType
    {
        ModelUploaded,
        ModelLiked,
        ModelDownloaded,
        CommentAdded,
        CommentLiked,
        CollectionCreated,
        CollectionUpdated,
        UserFollowed,
        SystemMaintenance,
        SystemUpdate,
        SecurityAlert,
        Welcome,
        Custom
    }

    public enum NotificationPriority
    {
        Low,
        Normal,
        High,
        Urgent
    }
}
