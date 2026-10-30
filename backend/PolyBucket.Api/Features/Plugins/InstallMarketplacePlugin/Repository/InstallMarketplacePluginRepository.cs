using PolyBucket.Api.Features.Plugins.Domain;
using PolyBucket.Api.Features.Plugins.Services;

namespace PolyBucket.Api.Features.Plugins.InstallMarketplacePlugin.Repository;

public class InstallMarketplacePluginRepository(
    MarketplaceClient marketplaceClient,
    PluginInstallationService installationService) : IInstallMarketplacePluginRepository
{
    public Task<MarketplacePluginDetails?> GetPluginDetailsAsync(string pluginId, CancellationToken cancellationToken = default) =>
        marketplaceClient.GetPluginDetailsAsync(pluginId);

    public Task<PluginInstallationResult> InstallFromMarketplaceAsync(MarketplacePluginDetails pluginDetails, CancellationToken cancellationToken = default) =>
        installationService.InstallFromMarketplaceAsync(pluginDetails);

    public Task RecordInstallationAsync(string pluginId, string version, Guid userId, CancellationToken cancellationToken = default) =>
        marketplaceClient.RecordInstallationAsync(pluginId, version, userId);
}
