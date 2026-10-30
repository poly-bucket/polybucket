using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PolyBucket.Api.Features.Users.Follow.Domain;
using PolyBucket.Api.Features.Users.GetUserFollowing.Repository;

namespace PolyBucket.Api.Features.Users.GetUserFollowing.Domain;

public class GetUserFollowingService(IGetUserFollowingRepository repository) : IGetUserFollowingService
{
    public async Task<(GetUserFollowingOutcome Outcome, GetUserFollowingResult? Result)> GetFollowingAsync(
        GetUserFollowingQuery query,
        CancellationToken cancellationToken = default)
    {
        var user = await repository.ResolveUserAsync(query, cancellationToken);
        if (user == null)
        {
            return (GetUserFollowingOutcome.UserNotFound, null);
        }

        if (!UserFollowAccess.CanViewFollowLists(user, query.RequestingUserId, query.IsRequestingUserAdmin))
        {
            return (GetUserFollowingOutcome.ProfilePrivate, null);
        }

        var (items, totalCount) = await repository.GetFollowingPageAsync(
            user.Id,
            query.Page,
            query.PageSize,
            cancellationToken);

        var totalPages = query.PageSize > 0
            ? (int)Math.Ceiling((double)totalCount / query.PageSize)
            : 0;

        var result = new GetUserFollowingResult
        {
            Users = items.Select(i => new UserFollowingListItemDto
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

        return (GetUserFollowingOutcome.Success, result);
    }
}
