using PolyBucket.Api.Features.Reports.GetReportAnalytics.Repository;
using PolyBucket.Api.Features.Reports.GetReportTrends.Http;
namespace PolyBucket.Api.Features.Reports.GetReportTrends.Repository;
public class GetReportTrendsRepository(IGetReportAnalyticsRepository analyticsRepository) : IGetReportTrendsRepository
{
    public async Task<ReportTrendsResponseDto> GetReportTrendsAsync(string period, int days, CancellationToken cancellationToken = default)
    {
        var trends = await analyticsRepository.GetReportTrendsAsync(period, days, cancellationToken);
        return new ReportTrendsResponseDto { Trends = trends.Select(t => new ReportTrendItemDto { Date = t.Date, TotalReports = t.TotalReports, ResolvedReports = t.ResolvedReports, ActiveReports = t.ActiveReports }).ToList() };
    }
}
