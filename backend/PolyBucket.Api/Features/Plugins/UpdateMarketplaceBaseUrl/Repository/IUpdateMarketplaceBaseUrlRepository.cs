namespace PolyBucket.Api.Features.Plugins.UpdateMarketplaceBaseUrl.Repository;

public interface IUpdateMarketplaceBaseUrlRepository
{
    Task SaveBaseUrlAsync(string normalizedBaseUrl, CancellationToken cancellationToken = default);
}
