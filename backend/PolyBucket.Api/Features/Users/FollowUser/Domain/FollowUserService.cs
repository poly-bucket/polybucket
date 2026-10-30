using System;
using System.Threading;
using System.Threading.Tasks;
using PolyBucket.Api.Features.Notifications.Domain;
using PolyBucket.Api.Features.Users.FollowUser.Repository;

namespace PolyBucket.Api.Features.Users.FollowUser.Domain;

public class FollowUserService(
    IFollowUserRepository repository,
    INotificationPublisher notificationPublisher,
    TimeProvider timeProvider) : IFollowUserService
{
    public async Task<FollowUserOutcome> FollowAsync(
        Guid followerUserId,
        Guid followedUserId,
        CancellationToken cancellationToken = default)
    {
        if (followerUserId == followedUserId)
        {
            return FollowUserOutcome.SelfFollow;
        }

        var target = await repository.GetActiveTargetUserAsync(followedUserId, cancellationToken);

        if (target == null)
        {
            return FollowUserOutcome.TargetNotFound;
        }

        if (target.IsBanned)
        {
            return FollowUserOutcome.TargetBanned;
        }

        if (!target.IsProfilePublic)
        {
            return FollowUserOutcome.TargetPrivate;
        }

        var existing = await repository.GetActiveFollowAsync(followerUserId, followedUserId, cancellationToken);
        if (existing != null)
        {
            return FollowUserOutcome.AlreadyFollowing;
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var follow = await repository.AddFollowAsync(followerUserId, followedUserId, followerUserId, now, cancellationToken);

        await notificationPublisher.PublishAsync(new NotificationRequest
        {
            RecipientUserId = followedUserId,
            ActorUserId = followerUserId,
            Type = NotificationType.UserFollowed,
            Title = $"{NotificationRequest.ActorToken} started following you",
            Message = $"{NotificationRequest.ActorToken} is now following you.",
            ActionUrl = $"/profile/{target.Username}",
            RelatedEntityId = followerUserId,
            RelatedEntityType = "User",
            Priority = NotificationPriority.Low,
            DedupeKey = $"follow:{follow.Id}",
            SendEmail = false
        }, cancellationToken);

        await repository.SaveChangesAsync(cancellationToken);

        return FollowUserOutcome.Success;
    }
}
