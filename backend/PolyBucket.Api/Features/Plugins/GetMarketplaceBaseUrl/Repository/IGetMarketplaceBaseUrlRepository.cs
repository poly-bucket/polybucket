namespace PolyBucket.Api.Features.Plugins.GetMarketplaceBaseUrl.Repository;

public interface IGetMarketplaceBaseUrlRepository
{
    Task<string?> GetBaseUrlAsync(CancellationToken cancellationToken = default);
}
