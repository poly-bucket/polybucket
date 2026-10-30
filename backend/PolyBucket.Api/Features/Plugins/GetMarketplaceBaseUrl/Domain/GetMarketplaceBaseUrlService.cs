using PolyBucket.Api.Features.Plugins.GetMarketplaceBaseUrl.Repository;

namespace PolyBucket.Api.Features.Plugins.GetMarketplaceBaseUrl.Domain;

public class GetMarketplaceBaseUrlService(IGetMarketplaceBaseUrlRepository repository) : IGetMarketplaceBaseUrlService
{
    public async Task<string> GetBaseUrlAsync(CancellationToken cancellationToken = default)
    {
        var value = await repository.GetBaseUrlAsync(cancellationToken);
        return value ?? string.Empty;
    }
}
