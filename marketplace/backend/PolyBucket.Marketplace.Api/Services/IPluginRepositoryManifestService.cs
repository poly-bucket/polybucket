using PolyBucket.Marketplace.Api.Models;

namespace PolyBucket.Marketplace.Api.Services;

public interface IPluginRepositoryManifestService
{
    Task<RemoteManifestReadResult> ReadManifestFromRepositoryAsync(
        string repositoryUrl,
        string branch,
        string? repositorySubpath,
        CancellationToken cancellationToken = default);
}
