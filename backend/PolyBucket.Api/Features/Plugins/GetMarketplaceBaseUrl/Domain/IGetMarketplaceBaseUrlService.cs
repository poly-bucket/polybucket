namespace PolyBucket.Api.Features.Plugins.GetMarketplaceBaseUrl.Domain;

public interface IGetMarketplaceBaseUrlService
{
    Task<string> GetBaseUrlAsync(CancellationToken cancellationToken = default);
}
