using PolyBucket.Api.Features.Reports.Domain;
using PolyBucket.Api.Features.Reports.GetReportAnalytics.Repository;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Reports.GetReportAnalytics.Domain
{
    public class GetReportAnalyticsService(IGetReportAnalyticsRepository repository) : IGetReportAnalyticsService
    {
        private readonly IGetReportAnalyticsRepository _repository = repository;

        public Task<ReportsAnalytics> GetReportsAnalyticsAsync(DateTime? fromDate, DateTime? toDate, CancellationToken cancellationToken = default)
        {
            return _repository.GetReportsAnalyticsAsync(fromDate, toDate, cancellationToken);
        }

        public Task<List<TopReportedItem>> GetTopReportedModelsAsync(int limit, CancellationToken cancellationToken = default)
        {
            return _repository.GetTopReportedModelsAsync(limit, cancellationToken);
        }

        public Task<List<TopReportedItem>> GetTopReportedUsersAsync(int limit, CancellationToken cancellationToken = default)
        {
            return _repository.GetTopReportedUsersAsync(limit, cancellationToken);
        }

        public Task<List<TopReportedItem>> GetTopReportedCommentsAsync(int limit, CancellationToken cancellationToken = default)
        {
            return _repository.GetTopReportedCommentsAsync(limit, cancellationToken);
        }

        public Task<List<ReportTrend>> GetReportTrendsAsync(string period, int days, CancellationToken cancellationToken = default)
        {
            return _repository.GetReportTrendsAsync(period, days, cancellationToken);
        }

        public Task<List<ModeratorActivity>> GetModeratorActivityAsync(DateTime? fromDate, DateTime? toDate, CancellationToken cancellationToken = default)
        {
            return _repository.GetModeratorActivityAsync(fromDate, toDate, cancellationToken);
        }
    }
}
