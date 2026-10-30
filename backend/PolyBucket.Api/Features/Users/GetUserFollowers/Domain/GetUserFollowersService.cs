using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PolyBucket.Api.Features.Users.Follow.Domain;
using PolyBucket.Api.Features.Users.GetUserFollowers.Repository;

namespace PolyBucket.Api.Features.Users.GetUserFollowers.Domain;

public class GetUserFollowersService(IGetUserFollowersRepository repository) : IGetUserFollowersService
{
    public async Task<(GetUserFollowListOutcome Outcome, GetUserFollowListResult? Result)> GetFollowersAsync(
        GetUserFollowListQuery query,
        CancellationToken cancellationToken = default)
    {
        var user = await repository.ResolveUserAsync(query, cancellationToken);
        if (user == null)
        {
            return (GetUserFollowListOutcome.UserNotFound, null);
        }

        if (!UserFollowAccess.CanViewFollowLists(user, query.RequestingUserId, query.IsRequestingUserAdmin))
        {
            return (GetUserFollowListOutcome.ProfilePrivate, null);
        }

        var (items, totalCount) = await repository.GetFollowersPageAsync(
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
}
