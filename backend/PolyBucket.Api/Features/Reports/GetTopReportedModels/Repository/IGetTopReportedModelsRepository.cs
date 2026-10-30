using PolyBucket.Api.Features.Reports.GetTopReportedModels.Http;

namespace PolyBucket.Api.Features.Reports.GetTopReportedModels.Repository;

public interface IGetTopReportedModelsRepository
{
    Task<TopReportedModelsResponseDto> GetTopReportedModelsAsync(int limit, CancellationToken cancellationToken = default);
}
