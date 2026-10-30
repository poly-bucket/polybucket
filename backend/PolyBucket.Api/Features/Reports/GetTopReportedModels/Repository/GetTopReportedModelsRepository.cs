using PolyBucket.Api.Features.Reports.Domain;
using PolyBucket.Api.Features.Reports.GetReportAnalytics.Repository;
using PolyBucket.Api.Features.Reports.GetTopReportedModels.Http;

namespace PolyBucket.Api.Features.Reports.GetTopReportedModels.Repository;

public class GetTopReportedModelsRepository(IGetReportAnalyticsRepository analyticsRepository) : IGetTopReportedModelsRepository
{
    public async Task<TopReportedModelsResponseDto> GetTopReportedModelsAsync(int limit, CancellationToken cancellationToken = default)
    {
        var items = await analyticsRepository.GetTopReportedModelsAsync(limit, cancellationToken);
        return new TopReportedModelsResponseDto
        {
            Items = items.Select(i => new TopReportedModelItemDto
            {
                Id = i.Id,
                Name = i.Name,
                ReportCount = i.ReportCount,
                LastReported = i.LastReported,
                IsResolved = i.IsResolved,
                Resolution = i.Resolution
            }).ToList()
        };
    }
}
