using PolyBucket.Api.Features.Reports.Domain;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Reports.GetReportAnalytics.Repository
{
    public interface IGetReportAnalyticsRepository
    {
        Task<ReportsAnalytics> GetReportsAnalyticsAsync(DateTime? fromDate, DateTime? toDate, CancellationToken cancellationToken = default);
        Task<List<TopReportedItem>> GetTopReportedModelsAsync(int limit, CancellationToken cancellationToken = default);
        Task<List<TopReportedItem>> GetTopReportedUsersAsync(int limit, CancellationToken cancellationToken = default);
        Task<List<TopReportedItem>> GetTopReportedCommentsAsync(int limit, CancellationToken cancellationToken = default);
        Task<List<ReportTrend>> GetReportTrendsAsync(string period, int days, CancellationToken cancellationToken = default);
        Task<List<ModeratorActivity>> GetModeratorActivityAsync(DateTime? fromDate, DateTime? toDate, CancellationToken cancellationToken = default);
    }
}
