using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Users.GetUserFollowing.Domain;

public interface IGetUserFollowingService
{
    Task<(GetUserFollowingOutcome Outcome, GetUserFollowingResult? Result)> GetFollowingAsync(
        GetUserFollowingQuery query,
        CancellationToken cancellationToken = default);
}
