using System;
using System.Collections.Generic;
using System.Linq;
using PolyBucket.Api.Features.Users.Follow.Repository;

namespace PolyBucket.Api.Features.Users.Follow.Domain;

public static class FollowerHistoryCalculator
{
    public static IReadOnlyList<FollowerHistoryRow> BuildSeries(
        IReadOnlyList<(DateTime CreatedAt, DateTime? DeletedAt)> follows,
        DateTime windowStartUtc,
        DateTime windowEndUtcExclusive)
    {
        var points = new List<FollowerHistoryRow>();
        for (var day = windowStartUtc.Date; day < windowEndUtcExclusive.Date; day = day.AddDays(1))
        {
            var dayEnd = day.AddDays(1);
            var count = follows.Count(f =>
                f.CreatedAt < dayEnd && (f.DeletedAt == null || f.DeletedAt >= dayEnd));
            points.Add(new FollowerHistoryRow { DateUtc = day, FollowerCount = count });
        }

        return points;
    }
}
