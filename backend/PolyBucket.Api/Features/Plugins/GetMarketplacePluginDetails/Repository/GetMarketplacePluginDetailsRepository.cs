using PolyBucket.Api.Features.Plugins.Services;

namespace PolyBucket.Api.Features.Plugins.GetMarketplacePluginDetails.Repository;

public class GetMarketplacePluginDetailsRepository(MarketplaceClient marketplaceClient) : IGetMarketplacePluginDetailsRepository
{
    public Task<MarketplacePluginDetails?> GetPluginDetailsAsync(string pluginId, CancellationToken cancellationToken = default) =>
        marketplaceClient.GetPluginDetailsAsync(pluginId);
}
