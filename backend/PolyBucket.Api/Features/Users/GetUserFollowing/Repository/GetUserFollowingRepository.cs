using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Features.Users.Follow.Repository;
using PolyBucket.Api.Features.Users.GetUserFollowing.Domain;
using PolyBucket.Database.Entities.Models;
using PolyBucket.Database.Persistence;

namespace PolyBucket.Api.Features.Users.GetUserFollowing.Repository;

public interface IGetUserFollowingRepository
{
    Task<User?> ResolveUserAsync(GetUserFollowingQuery query, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<UserFollowListRow> Items, int TotalCount)> GetFollowingPageAsync(
        Guid followerUserId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}

public class GetUserFollowingRepository(PolyBucketDbContext dbContext, IUserFollowRepository followRepository) : IGetUserFollowingRepository
{
    public async Task<User?> ResolveUserAsync(GetUserFollowingQuery query, CancellationToken cancellationToken = default)
    {
        if (query.UserId != Guid.Empty)
        {
            return await dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == query.UserId && u.DeletedAt == null, cancellationToken);
        }

        if (!string.IsNullOrWhiteSpace(query.Username))
        {
            return await dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Username == query.Username && u.DeletedAt == null, cancellationToken);
        }

        return null;
    }

    public Task<(IReadOnlyList<UserFollowListRow> Items, int TotalCount)> GetFollowingPageAsync(
        Guid followerUserId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default) =>
        followRepository.GetFollowingPageAsync(followerUserId, page, pageSize, cancellationToken);
}
