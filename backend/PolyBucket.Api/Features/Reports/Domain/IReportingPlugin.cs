using PolyBucket.Api.Common.Plugins;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Reports.Domain
{
    public enum ReportType
    {
        Model,
        Comment
    }

    public enum ReportReason
    {
        Inappropriate,
        Spam,
        Copyright,
        Other
    }

    public class ReportsResponse
    {
        public IEnumerable<Report> Reports { get; set; } = new List<Report>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public interface IReportingPlugin : IPlugin
    {
        Task<Report> SubmitReportAsync(ReportType type, Guid targetId, Guid reporterId, ReportReason reason, string description);
        Task<IEnumerable<Report>> GetReportsForTargetAsync(ReportType type, Guid targetId);
        Task<IEnumerable<Report>> GetUnresolvedReportsAsync();
        Task<ReportsResponse> GetAllReportsAsync(int page = 1, int pageSize = 20, bool? isResolved = null, ReportType? type = null);
        Task ResolveReportAsync(Guid reportId, Guid resolverId, string resolution);
        Task<Report?> GetReportByIdAsync(Guid reportId);
        Task<ReportsAnalytics> GetReportsAnalyticsAsync(DateTime? fromDate = null, DateTime? toDate = null);
        Task<List<TopReportedItem>> GetTopReportedModelsAsync(int limit = 10);
        Task<List<TopReportedItem>> GetTopReportedUsersAsync(int limit = 10);
        Task<List<TopReportedItem>> GetTopReportedCommentsAsync(int limit = 10);
        Task<List<ReportTrend>> GetReportTrendsAsync(string period = "daily", int days = 30);
        Task<List<ModeratorActivity>> GetModeratorActivityAsync(DateTime? fromDate = null, DateTime? toDate = null);
    }
}
