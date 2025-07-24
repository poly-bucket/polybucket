using System;
using System.Collections.Generic;

namespace PolyBucket.Api.Features.Reports.Domain
{
    public class ReportsAnalytics
    {
        public int TotalReports { get; set; }
        public int ActiveReports { get; set; }
        public int ResolvedReports { get; set; }
        public int DismissedReports { get; set; }
        public int ArchivedReports { get; set; }
        public DateTime LastUpdated { get; set; }
        
        public List<ReportTrend> DailyTrends { get; set; } = new();
        public List<ReportTrend> WeeklyTrends { get; set; } = new();
        public List<ReportTrend> MonthlyTrends { get; set; } = new();
        
        public List<TopReportedItem> TopReportedModels { get; set; } = new();
        public List<TopReportedItem> TopReportedUsers { get; set; } = new();
        public List<TopReportedItem> TopReportedComments { get; set; } = new();
        
        public List<ReportReasonStats> ReasonStatistics { get; set; } = new();
        public List<ReportTypeStats> TypeStatistics { get; set; } = new();
        
        public List<ModeratorActivity> ModeratorActivity { get; set; } = new();
    }

    public class ReportTrend
    {
        public DateTime Date { get; set; }
        public int TotalReports { get; set; }
        public int ResolvedReports { get; set; }
        public int ActiveReports { get; set; }
    }

    public class TopReportedItem
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int ReportCount { get; set; }
        public DateTime LastReported { get; set; }
        public bool IsResolved { get; set; }
        public string? Resolution { get; set; }
    }

    public class ReportReasonStats
    {
        public ReportReason Reason { get; set; }
        public int Count { get; set; }
        public double Percentage { get; set; }
    }

    public class ReportTypeStats
    {
        public ReportType Type { get; set; }
        public int Count { get; set; }
        public double Percentage { get; set; }
    }

    public class ModeratorActivity
    {
        public Guid ModeratorId { get; set; }
        public string ModeratorName { get; set; } = string.Empty;
        public int ReportsResolved { get; set; }
        public int ReportsDismissed { get; set; }
        public int ReportsArchived { get; set; }
        public DateTime LastActivity { get; set; }
        public double AverageResolutionTime { get; set; } // in hours
    }
} 