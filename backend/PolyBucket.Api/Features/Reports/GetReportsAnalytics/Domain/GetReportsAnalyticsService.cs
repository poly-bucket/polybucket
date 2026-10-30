using PolyBucket.Api.Features.Reports.GetReportsAnalytics.Http;
using PolyBucket.Api.Features.Reports.GetReportsAnalytics.Repository;

namespace PolyBucket.Api.Features.Reports.GetReportsAnalytics.Domain;

public interface IGetReportsAnalyticsService
{
    Task<ReportsAnalyticsResponseDto> GetReportsAnalyticsAsync(DateTime? fromDate, DateTime? toDate, CancellationToken cancellationToken = default);
}

public class GetReportsAnalyticsService(IGetReportsAnalyticsRepository repository) : IGetReportsAnalyticsService
{
    public Task<ReportsAnalyticsResponseDto> GetReportsAnalyticsAsync(DateTime? fromDate, DateTime? toDate, CancellationToken cancellationToken = default) =>
        repository.GetReportsAnalyticsAsync(fromDate, toDate, cancellationToken);
}
