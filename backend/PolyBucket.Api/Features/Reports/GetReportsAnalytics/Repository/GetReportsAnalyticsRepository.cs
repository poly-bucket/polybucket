using PolyBucket.Api.Features.Reports.Domain;
using PolyBucket.Api.Features.Reports.GetReportAnalytics.Repository;
using PolyBucket.Api.Features.Reports.GetReportsAnalytics.Http;

namespace PolyBucket.Api.Features.Reports.GetReportsAnalytics.Repository;

public interface IGetReportsAnalyticsRepository
{
    Task<ReportsAnalyticsResponseDto> GetReportsAnalyticsAsync(DateTime? fromDate, DateTime? toDate, CancellationToken cancellationToken = default);
}

public class GetReportsAnalyticsRepository(IGetReportAnalyticsRepository analyticsRepository) : IGetReportsAnalyticsRepository
{
    public async Task<ReportsAnalyticsResponseDto> GetReportsAnalyticsAsync(DateTime? fromDate, DateTime? toDate, CancellationToken cancellationToken = default)
    {
        var analytics = await analyticsRepository.GetReportsAnalyticsAsync(fromDate, toDate, cancellationToken);
        return Map(analytics);
    }

    internal static ReportsAnalyticsResponseDto Map(ReportsAnalytics analytics) => new()
    {
        TotalReports = analytics.TotalReports,
        ActiveReports = analytics.ActiveReports,
        ResolvedReports = analytics.ResolvedReports,
        DismissedReports = analytics.DismissedReports,
        ArchivedReports = analytics.ArchivedReports,
        LastUpdated = analytics.LastUpdated,
        DailyTrends = analytics.DailyTrends.Select(t => new ReportTrendItemDto
        {
            Date = t.Date,
            TotalReports = t.TotalReports,
            ResolvedReports = t.ResolvedReports,
            ActiveReports = t.ActiveReports
        }).ToList(),
        WeeklyTrends = analytics.WeeklyTrends.Select(t => new ReportTrendItemDto
        {
            Date = t.Date,
            TotalReports = t.TotalReports,
            ResolvedReports = t.ResolvedReports,
            ActiveReports = t.ActiveReports
        }).ToList(),
        MonthlyTrends = analytics.MonthlyTrends.Select(t => new ReportTrendItemDto
        {
            Date = t.Date,
            TotalReports = t.TotalReports,
            ResolvedReports = t.ResolvedReports,
            ActiveReports = t.ActiveReports
        }).ToList(),
        TopReportedModels = analytics.TopReportedModels.Select(MapTop).ToList(),
        TopReportedUsers = analytics.TopReportedUsers.Select(MapTop).ToList(),
        TopReportedComments = analytics.TopReportedComments.Select(MapTop).ToList(),
        ReasonStatistics = analytics.ReasonStatistics.Select(r => new ReportReasonStatsDto
        {
            Reason = r.Reason,
            Count = r.Count,
            Percentage = r.Percentage
        }).ToList(),
        TypeStatistics = analytics.TypeStatistics.Select(t => new ReportTypeStatsDto
        {
            Type = t.Type,
            Count = t.Count,
            Percentage = t.Percentage
        }).ToList(),
        ModeratorActivity = analytics.ModeratorActivity.Select(m => new ModeratorActivityItemDto
        {
            ModeratorId = m.ModeratorId,
            ModeratorName = m.ModeratorName,
            ReportsResolved = m.ReportsResolved,
            ReportsDismissed = m.ReportsDismissed,
            ReportsArchived = m.ReportsArchived,
            LastActivity = m.LastActivity,
            AverageResolutionTime = m.AverageResolutionTime
        }).ToList()
    };

    private static TopReportedItemDto MapTop(TopReportedItem item) => new()
    {
        Id = item.Id,
        Name = item.Name,
        ReportCount = item.ReportCount,
        LastReported = item.LastReported,
        IsResolved = item.IsResolved,
        Resolution = item.Resolution
    };
}
