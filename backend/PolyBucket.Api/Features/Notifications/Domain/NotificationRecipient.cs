using System;

namespace PolyBucket.Api.Features.Notifications.Domain;

public sealed record NotificationRecipient(
    Guid UserId,
    string Username,
    string Email,
    bool EmailNotifications,
    bool NotifyOnLikes,
    bool NotifyOnComments,
    bool NotifyOnFollows)
{
    public bool Allows(NotificationType type) => type switch
    {
        NotificationType.ModelLiked or NotificationType.CommentLiked => NotifyOnLikes,
        NotificationType.CommentAdded or NotificationType.CommentReplied => NotifyOnComments,
        NotificationType.UserFollowed => NotifyOnFollows,
        _ => true
    };
}
