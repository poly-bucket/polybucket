using PolyBucket.Api.Features.Plugins.GetMarketplacePluginDetails.Repository;
using PolyBucket.Api.Features.Plugins.Services;

namespace PolyBucket.Api.Features.Plugins.GetMarketplacePluginDetails.Domain;

public class GetMarketplacePluginDetailsService(IGetMarketplacePluginDetailsRepository repository) : IGetMarketplacePluginDetailsService
{
    public Task<MarketplacePluginDetails?> GetPluginDetailsAsync(string pluginId, CancellationToken cancellationToken = default) =>
        repository.GetPluginDetailsAsync(pluginId, cancellationToken);
}
