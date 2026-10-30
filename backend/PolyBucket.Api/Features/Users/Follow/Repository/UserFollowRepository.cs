using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Features.Users.Follow.Domain;
using PolyBucket.Database.Entities.Users;
using PolyBucket.Database.Persistence;

namespace PolyBucket.Api.Features.Users.Follow.Repository;

public class UserFollowRepository(PolyBucketDbContext context) : IUserFollowRepository
{
    public Task<UserFollow?> GetActiveFollowAsync(Guid followerUserId, Guid followedUserId, CancellationToken cancellationToken = default)
    {
        return context.UserFollows
            .FirstOrDefaultAsync(
                f => f.FollowerUserId == followerUserId
                     && f.FollowedUserId == followedUserId
                     && f.DeletedAt == null,
                cancellationToken);
    }

    public async Task<UserFollow> AddFollowAsync(
        Guid followerUserId,
        Guid followedUserId,
        Guid actorUserId,
        DateTime now,
        CancellationToken cancellationToken = default)
    {
        var follow = new UserFollow
        {
            Id = Guid.NewGuid(),
            FollowerUserId = followerUserId,
            FollowedUserId = followedUserId,
            CreatedAt = now,
            CreatedById = actorUserId,
            UpdatedAt = now,
            UpdatedById = actorUserId
        };
        context.UserFollows.Add(follow);
        await context.SaveChangesAsync(cancellationToken);
        return follow;
    }

    public async Task<bool> SoftDeleteActiveFollowAsync(
        Guid followerUserId,
        Guid followedUserId,
        Guid actorUserId,
        DateTime now,
        CancellationToken cancellationToken = default)
    {
        var follow = await GetActiveFollowAsync(followerUserId, followedUserId, cancellationToken);
        if (follow == null)
        {
            return false;
        }

        follow.DeletedAt = now;
        follow.DeletedById = actorUserId;
        follow.UpdatedAt = now;
        follow.UpdatedById = actorUserId;
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public Task<int> CountActiveFollowersAsync(Guid followedUserId, CancellationToken cancellationToken = default)
    {
        return context.UserFollows
            .AsNoTracking()
            .CountAsync(f => f.FollowedUserId == followedUserId && f.DeletedAt == null, cancellationToken);
    }

    public Task<int> CountActiveFollowingAsync(Guid followerUserId, CancellationToken cancellationToken = default)
    {
        return context.UserFollows
            .AsNoTracking()
            .CountAsync(f => f.FollowerUserId == followerUserId && f.DeletedAt == null, cancellationToken);
    }

    public Task<bool> IsActiveFollowerAsync(Guid followerUserId, Guid followedUserId, CancellationToken cancellationToken = default)
    {
        return context.UserFollows
            .AsNoTracking()
            .AnyAsync(
                f => f.FollowerUserId == followerUserId
                     && f.FollowedUserId == followedUserId
                     && f.DeletedAt == null,
                cancellationToken);
    }

    public async Task<(IReadOnlyList<UserFollowListRow> Items, int TotalCount)> GetFollowersPageAsync(
        Guid followedUserId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query =
            from follow in context.UserFollows.AsNoTracking()
            join user in context.Users.AsNoTracking() on follow.FollowerUserId equals user.Id
            where follow.FollowedUserId == followedUserId
                  && follow.DeletedAt == null
                  && user.DeletedAt == null
            orderby follow.CreatedAt descending
            select new UserFollowListRow
            {
                Id = user.Id,
                Username = user.Username,
                Avatar = user.Avatar,
                FollowedAt = follow.CreatedAt
            };

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<(IReadOnlyList<UserFollowListRow> Items, int TotalCount)> GetFollowingPageAsync(
        Guid followerUserId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query =
            from follow in context.UserFollows.AsNoTracking()
            join user in context.Users.AsNoTracking() on follow.FollowedUserId equals user.Id
            where follow.FollowerUserId == followerUserId
                  && follow.DeletedAt == null
                  && user.DeletedAt == null
            orderby follow.CreatedAt descending
            select new UserFollowListRow
            {
                Id = user.Id,
                Username = user.Username,
                Avatar = user.Avatar,
                FollowedAt = follow.CreatedAt
            };

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<IReadOnlyList<FollowerHistoryRow>> GetFollowerHistoryAsync(
        Guid followedUserId,
        DateTime windowStartUtc,
        DateTime windowEndUtcExclusive,
        CancellationToken cancellationToken = default)
    {
        var follows = await context.UserFollows
            .AsNoTracking()
            .Where(f => f.FollowedUserId == followedUserId)
            .Select(f => new { f.CreatedAt, f.DeletedAt })
            .ToListAsync(cancellationToken);

        var tuples = follows.Select(f => (f.CreatedAt, f.DeletedAt)).ToList();
        return FollowerHistoryCalculator.BuildSeries(tuples, windowStartUtc, windowEndUtcExclusive);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        context.SaveChangesAsync(cancellationToken);
}
