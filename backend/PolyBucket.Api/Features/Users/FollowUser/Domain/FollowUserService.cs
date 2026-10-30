using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Features.Notifications.Domain;
using PolyBucket.Api.Features.Users.Follow.Repository;
using PolyBucket.Api.Features.Users.FollowUser.Domain;
using PolyBucket.Database.Persistence;

namespace PolyBucket.Api.Features.Users.FollowUser.Domain;

public class FollowUserService(
    PolyBucketDbContext dbContext,
    IUserFollowRepository followRepository,
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

        var target = await dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == followedUserId && u.DeletedAt == null, cancellationToken);

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

        var existing = await followRepository.GetActiveFollowAsync(followerUserId, followedUserId, cancellationToken);
        if (existing != null)
        {
            return FollowUserOutcome.AlreadyFollowing;
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var follow = await followRepository.AddFollowAsync(followerUserId, followedUserId, followerUserId, now, cancellationToken);

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

        await followRepository.SaveChangesAsync(cancellationToken);

        return FollowUserOutcome.Success;
    }
}
