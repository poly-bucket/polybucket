using System;
using System.Linq;
using PolyBucket.Api.Features.Users.Follow.Domain;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Users.Follow;

public class FollowerHistoryCalculatorTests
{
    [Fact(DisplayName = "When a follow is created on a day, the follower count rises for that day.")]
    public void BuildSeries_FollowOnDay_IncreasesCountForThatDay()
    {
        var day = new DateTime(2026, 1, 10, 0, 0, 0, DateTimeKind.Utc);
        var windowStart = day;
        var windowEnd = day.AddDays(2);
        var follows = new[] { (day.AddHours(12), (DateTime?)null) };

        var series = FollowerHistoryCalculator.BuildSeries(follows, windowStart, windowEnd);

        series.Single(p => p.DateUtc == day).FollowerCount.ShouldBe(1);
        series.Single(p => p.DateUtc == day.AddDays(1)).FollowerCount.ShouldBe(1);
    }

    [Fact(DisplayName = "When a follow ends on a day, later days show a lower follower count.")]
    public void BuildSeries_UnfollowOnDay_DropsCountOnLaterDays()
    {
        var day = new DateTime(2026, 1, 10, 0, 0, 0, DateTimeKind.Utc);
        var windowStart = day;
        var windowEnd = day.AddDays(3);
        var follows = new[] { (day.AddHours(8), (DateTime?)day.AddHours(20)) };

        var series = FollowerHistoryCalculator.BuildSeries(follows, windowStart, windowEnd);

        series.Single(p => p.DateUtc == day).FollowerCount.ShouldBe(0);
        series.Single(p => p.DateUtc == day.AddDays(1)).FollowerCount.ShouldBe(0);
        series.Single(p => p.DateUtc == day.AddDays(2)).FollowerCount.ShouldBe(0);
    }

    [Fact(DisplayName = "When a user refollows, the follower count rises again on later days.")]
    public void BuildSeries_Refollow_RaisesCountAgain()
    {
        var day = new DateTime(2026, 1, 10, 0, 0, 0, DateTimeKind.Utc);
        var windowStart = day;
        var windowEnd = day.AddDays(3);
        var follows = new[]
        {
            (day.AddHours(1), (DateTime?)day.AddHours(10)),
            (day.AddDays(1).AddHours(5), (DateTime?)null)
        };

        var series = FollowerHistoryCalculator.BuildSeries(follows, windowStart, windowEnd);

        series.Single(p => p.DateUtc == day).FollowerCount.ShouldBe(0);
        series.Single(p => p.DateUtc == day.AddDays(1)).FollowerCount.ShouldBe(1);
        series.Single(p => p.DateUtc == day.AddDays(2)).FollowerCount.ShouldBe(1);
    }

    [Fact(DisplayName = "When no follows exist, each day in the window has zero followers.")]
    public void BuildSeries_NoFollows_ReturnsZeroLine()
    {
        var day = new DateTime(2026, 1, 10, 0, 0, 0, DateTimeKind.Utc);
        var series = FollowerHistoryCalculator.BuildSeries(
            Array.Empty<(DateTime, DateTime?)>(),
            day,
            day.AddDays(3));

        series.Count.ShouldBe(3);
        series.All(p => p.FollowerCount == 0).ShouldBeTrue();
    }
}
