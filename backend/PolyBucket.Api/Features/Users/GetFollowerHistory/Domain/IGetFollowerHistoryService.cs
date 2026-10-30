using System.Threading;
using System.Threading.Tasks;
using PolyBucket.Api.Features.Users.GetFollowerHistory.Domain;

namespace PolyBucket.Api.Features.Users.GetFollowerHistory.Domain;

public interface IGetFollowerHistoryService
{
    Task<(GetFollowerHistoryOutcome Outcome, GetFollowerHistoryResult? Result)> GetHistoryAsync(
        GetFollowerHistoryQuery query,
        CancellationToken cancellationToken = default);
}
