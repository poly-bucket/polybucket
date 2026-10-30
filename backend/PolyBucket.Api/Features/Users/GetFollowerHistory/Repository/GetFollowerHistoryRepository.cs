using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Features.Users.Follow.Repository;
using PolyBucket.Database.Persistence;

namespace PolyBucket.Api.Features.Users.GetFollowerHistory.Repository;

public interface IGetFollowerHistoryRepository
{
    Task<bool> UserExistsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<FollowerHistoryRow>> GetFollowerHistoryAsync(
        Guid followedUserId,
        DateTime windowStartUtc,
        DateTime windowEndUtcExclusive,
        CancellationToken cancellationToken = default);
}

public class GetFollowerHistoryRepository(PolyBucketDbContext dbContext, IUserFollowRepository followRepository) : IGetFollowerHistoryRepository
{
    public Task<bool> UserExistsAsync(Guid userId, CancellationToken cancellationToken = default) =>
        dbContext.Users
            .AsNoTracking()
            .AnyAsync(u => u.Id == userId && u.DeletedAt == null, cancellationToken);

    public Task<IReadOnlyList<FollowerHistoryRow>> GetFollowerHistoryAsync(
        Guid followedUserId,
        DateTime windowStartUtc,
        DateTime windowEndUtcExclusive,
        CancellationToken cancellationToken = default) =>
        followRepository.GetFollowerHistoryAsync(followedUserId, windowStartUtc, windowEndUtcExclusive, cancellationToken);
}
