using System.Threading;
using System.Threading.Tasks;
using PolyBucket.Api.Features.Users.GetUserFollowers.Domain;

namespace PolyBucket.Api.Features.Users.GetUserFollowing.Domain;

public interface IGetUserFollowingService
{
    Task<(GetUserFollowListOutcome Outcome, GetUserFollowListResult? Result)> GetFollowingAsync(
        GetUserFollowListQuery query,
        CancellationToken cancellationToken = default);
}
