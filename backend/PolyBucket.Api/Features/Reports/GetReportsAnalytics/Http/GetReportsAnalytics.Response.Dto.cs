using PolyBucket.Api.Features.Reports.Domain;

namespace PolyBucket.Api.Features.Reports.GetReportsAnalytics.Http;

public class ReportsAnalyticsResponseDto
{
    public int TotalReports { get; set; }
    public int ActiveReports { get; set; }
    public int ResolvedReports { get; set; }
    public int DismissedReports { get; set; }
    public int ArchivedReports { get; set; }
    public DateTime LastUpdated { get; set; }
    public List<ReportTrendItemDto> DailyTrends { get; set; } = new();
    public List<ReportTrendItemDto> WeeklyTrends { get; set; } = new();
    public List<ReportTrendItemDto> MonthlyTrends { get; set; } = new();
    public List<TopReportedItemDto> TopReportedModels { get; set; } = new();
    public List<TopReportedItemDto> TopReportedUsers { get; set; } = new();
    public List<TopReportedItemDto> TopReportedComments { get; set; } = new();
    public List<ReportReasonStatsDto> ReasonStatistics { get; set; } = new();
    public List<ReportTypeStatsDto> TypeStatistics { get; set; } = new();
    public List<ModeratorActivityItemDto> ModeratorActivity { get; set; } = new();
}

public class ReportTrendItemDto
{
    public DateTime Date { get; set; }
    public int TotalReports { get; set; }
    public int ResolvedReports { get; set; }
    public int ActiveReports { get; set; }
}

public class TopReportedItemDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int ReportCount { get; set; }
    public DateTime LastReported { get; set; }
    public bool IsResolved { get; set; }
    public string? Resolution { get; set; }
}

public class ReportReasonStatsDto
{
    public ReportReason Reason { get; set; }
    public int Count { get; set; }
    public double Percentage { get; set; }
}

public class ReportTypeStatsDto
{
    public ReportType Type { get; set; }
    public int Count { get; set; }
    public double Percentage { get; set; }
}

public class ModeratorActivityItemDto
{
    public Guid ModeratorId { get; set; }
    public string ModeratorName { get; set; } = string.Empty;
    public int ReportsResolved { get; set; }
    public int ReportsDismissed { get; set; }
    public int ReportsArchived { get; set; }
    public DateTime LastActivity { get; set; }
    public double AverageResolutionTime { get; set; }
}
