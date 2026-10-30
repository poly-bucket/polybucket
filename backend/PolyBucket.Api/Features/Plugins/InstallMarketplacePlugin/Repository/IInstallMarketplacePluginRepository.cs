using PolyBucket.Api.Features.Plugins.Domain;
using PolyBucket.Api.Features.Plugins.Services;

namespace PolyBucket.Api.Features.Plugins.InstallMarketplacePlugin.Repository;

public interface IInstallMarketplacePluginRepository
{
    Task<MarketplacePluginDetails?> GetPluginDetailsAsync(string pluginId, CancellationToken cancellationToken = default);

    Task<PluginInstallationResult> InstallFromMarketplaceAsync(MarketplacePluginDetails pluginDetails, CancellationToken cancellationToken = default);

    Task RecordInstallationAsync(string pluginId, string version, Guid userId, CancellationToken cancellationToken = default);
}
