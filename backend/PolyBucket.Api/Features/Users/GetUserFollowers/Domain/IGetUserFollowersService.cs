using System.Threading;
using System.Threading.Tasks;
using PolyBucket.Api.Features.Users.GetUserFollowers.Domain;

namespace PolyBucket.Api.Features.Users.GetUserFollowers.Domain;

public interface IGetUserFollowersService
{
    Task<(GetUserFollowListOutcome Outcome, GetUserFollowListResult? Result)> GetFollowersAsync(
        GetUserFollowListQuery query,
        CancellationToken cancellationToken = default);
}
