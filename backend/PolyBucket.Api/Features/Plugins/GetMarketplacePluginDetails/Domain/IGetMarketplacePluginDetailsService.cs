using PolyBucket.Api.Features.Plugins.Services;

namespace PolyBucket.Api.Features.Plugins.GetMarketplacePluginDetails.Domain;

public interface IGetMarketplacePluginDetailsService
{
    Task<MarketplacePluginDetails?> GetPluginDetailsAsync(string pluginId, CancellationToken cancellationToken = default);
}
