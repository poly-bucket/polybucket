using PolyBucket.Api.Features.Reports.GetTopReportedModels.Http;
using PolyBucket.Api.Features.Reports.GetTopReportedModels.Repository;

namespace PolyBucket.Api.Features.Reports.GetTopReportedModels.Domain;

public interface IGetTopReportedModelsService
{
    Task<TopReportedModelsResponseDto> GetTopReportedModelsAsync(int limit, CancellationToken cancellationToken = default);
}

public class GetTopReportedModelsService(IGetTopReportedModelsRepository repository) : IGetTopReportedModelsService
{
    public Task<TopReportedModelsResponseDto> GetTopReportedModelsAsync(int limit, CancellationToken cancellationToken = default) =>
        repository.GetTopReportedModelsAsync(limit, cancellationToken);
}
