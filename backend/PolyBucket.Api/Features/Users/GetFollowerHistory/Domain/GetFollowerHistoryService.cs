using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PolyBucket.Api.Features.Users.GetFollowerHistory.Repository;

namespace PolyBucket.Api.Features.Users.GetFollowerHistory.Domain;

public class GetFollowerHistoryService(
    IGetFollowerHistoryRepository repository,
    TimeProvider timeProvider) : IGetFollowerHistoryService
{
    private static readonly int[] AllowedDayWindows = [7, 30, 90];

    public async Task<(GetFollowerHistoryOutcome Outcome, GetFollowerHistoryResult? Result)> GetHistoryAsync(
        GetFollowerHistoryQuery query,
        CancellationToken cancellationToken = default)
    {
        if (!AllowedDayWindows.Contains(query.Days))
        {
            return (GetFollowerHistoryOutcome.InvalidDays, null);
        }

        var userExists = await repository.UserExistsAsync(query.UserId, cancellationToken);

        if (!userExists)
        {
            return (GetFollowerHistoryOutcome.UserNotFound, null);
        }

        if (query.RequestingUserId != query.UserId)
        {
            return (GetFollowerHistoryOutcome.Forbidden, null);
        }

        var today = timeProvider.GetUtcNow().UtcDateTime.Date;
        var windowStart = today.AddDays(-(query.Days - 1));
        var windowEndExclusive = today.AddDays(1);

        var rows = await repository.GetFollowerHistoryAsync(
            query.UserId,
            windowStart,
            windowEndExclusive,
            cancellationToken);

        var result = new GetFollowerHistoryResult
        {
            Days = query.Days,
            Points = rows.Select(r => new FollowerHistoryPointDto
            {
                Date = r.DateUtc,
                FollowerCount = r.FollowerCount
            }).ToList()
        };

        return (GetFollowerHistoryOutcome.Success, result);
    }
}
