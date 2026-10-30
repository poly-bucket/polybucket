using PolyBucket.Api.Features.Reports.GetReportTrends.Http;
using PolyBucket.Api.Features.Reports.GetReportTrends.Repository;
namespace PolyBucket.Api.Features.Reports.GetReportTrends.Domain;
public interface IGetReportTrendsService { Task<ReportTrendsResponseDto> GetReportTrendsAsync(string period, int days, CancellationToken cancellationToken = default); }
public class GetReportTrendsService(IGetReportTrendsRepository repository) : IGetReportTrendsService {
    public Task<ReportTrendsResponseDto> GetReportTrendsAsync(string period, int days, CancellationToken cancellationToken = default) => repository.GetReportTrendsAsync(period, days, cancellationToken);
}
