using PolyBucket.Api.Features.Reports.GetReportTrends.Http;
namespace PolyBucket.Api.Features.Reports.GetReportTrends.Repository;
public interface IGetReportTrendsRepository
{
    Task<ReportTrendsResponseDto> GetReportTrendsAsync(string period, int days, CancellationToken cancellationToken = default);
}
