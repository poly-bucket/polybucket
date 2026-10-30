using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PolyBucket.Database.Entities.Users;

namespace PolyBucket.Api.Features.Users.Follow.Repository;

public interface IUserFollowRepository
{
    Task<UserFollow?> GetActiveFollowAsync(Guid followerUserId, Guid followedUserId, CancellationToken cancellationToken = default);
    Task<UserFollow> AddFollowAsync(Guid followerUserId, Guid followedUserId, Guid actorUserId, DateTime now, CancellationToken cancellationToken = default);
    Task<bool> SoftDeleteActiveFollowAsync(Guid followerUserId, Guid followedUserId, Guid actorUserId, DateTime now, CancellationToken cancellationToken = default);
    Task<int> CountActiveFollowersAsync(Guid followedUserId, CancellationToken cancellationToken = default);
    Task<int> CountActiveFollowingAsync(Guid followerUserId, CancellationToken cancellationToken = default);
    Task<bool> IsActiveFollowerAsync(Guid followerUserId, Guid followedUserId, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<UserFollowListRow> Items, int TotalCount)> GetFollowersPageAsync(
        Guid followedUserId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<UserFollowListRow> Items, int TotalCount)> GetFollowingPageAsync(
        Guid followerUserId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<FollowerHistoryRow>> GetFollowerHistoryAsync(
        Guid followedUserId,
        DateTime windowStartUtc,
        DateTime windowEndUtcExclusive,
        CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

public sealed class UserFollowListRow
{
    public Guid Id { get; init; }
    public string Username { get; init; } = string.Empty;
    public string? Avatar { get; init; }
    public DateTime FollowedAt { get; init; }
}

public sealed class FollowerHistoryRow
{
    public DateTime DateUtc { get; init; }
    public int FollowerCount { get; init; }
}
