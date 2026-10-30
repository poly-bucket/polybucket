using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Features.Users.Follow.Domain;
using PolyBucket.Api.Features.Users.Follow.Repository;
using PolyBucket.Api.Features.Users.GetUserFollowers.Domain;
using PolyBucket.Database.Persistence;

namespace PolyBucket.Api.Features.Users.GetUserFollowers.Domain;

public class GetUserFollowersService(
    PolyBucketDbContext dbContext,
    IUserFollowRepository followRepository) : IGetUserFollowersService
{
    public async Task<(GetUserFollowListOutcome Outcome, GetUserFollowListResult? Result)> GetFollowersAsync(
        GetUserFollowListQuery query,
        CancellationToken cancellationToken = default)
    {
        var user = await ResolveUserAsync(query, cancellationToken);
        if (user == null)
        {
            return (GetUserFollowListOutcome.UserNotFound, null);
        }

        if (!UserFollowAccess.CanViewFollowLists(user, query.RequestingUserId, query.IsRequestingUserAdmin))
        {
            return (GetUserFollowListOutcome.ProfilePrivate, null);
        }

        var (items, totalCount) = await followRepository.GetFollowersPageAsync(
            user.Id,
            query.Page,
            query.PageSize,
            cancellationToken);

        var totalPages = query.PageSize > 0
            ? (int)Math.Ceiling((double)totalCount / query.PageSize)
            : 0;

        var result = new GetUserFollowListResult
        {
            Users = items.Select(i => new UserFollowListItemDto
            {
                Id = i.Id,
                Username = i.Username,
                Avatar = i.Avatar,
                FollowedAt = i.FollowedAt
            }).ToList(),
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalPages = totalPages
        };

        return (GetUserFollowListOutcome.Success, result);
    }

    private async Task<PolyBucket.Database.Entities.Models.User?> ResolveUserAsync(
        GetUserFollowListQuery query,
        CancellationToken cancellationToken)
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
}
