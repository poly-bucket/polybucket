using PolyBucket.Api.Features.Models.DownloadModel.Domain;

namespace PolyBucket.Api.Features.Models.DownloadModel.Repository;

public interface IDownloadModelRepository
{
    Task<DownloadModelBundle?> GetBundleForDownloadAsync(Guid modelId, CancellationToken cancellationToken = default);
}
