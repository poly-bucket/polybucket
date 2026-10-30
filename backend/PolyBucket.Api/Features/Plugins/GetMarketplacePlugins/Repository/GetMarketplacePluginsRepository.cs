using PolyBucket.Api.Features.Plugins.Services;

namespace PolyBucket.Api.Features.Plugins.GetMarketplacePlugins.Repository;

public class GetMarketplacePluginsRepository(MarketplaceClient marketplaceClient) : IGetMarketplacePluginsRepository
{
    public Task<MarketplacePluginsResponse> GetPluginsAsync(int page, int pageSize, string? category, string? search, string? target, CancellationToken cancellationToken = default) =>
        marketplaceClient.GetPluginsAsync(page, pageSize, category, search, target);
}
