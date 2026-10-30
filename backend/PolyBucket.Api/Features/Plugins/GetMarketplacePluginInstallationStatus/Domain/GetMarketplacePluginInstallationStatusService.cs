using PolyBucket.Api.Features.Plugins.GetMarketplacePluginInstallationStatus.Repository;

namespace PolyBucket.Api.Features.Plugins.GetMarketplacePluginInstallationStatus.Domain;

public class GetMarketplacePluginInstallationStatusService(IGetMarketplacePluginInstallationStatusRepository repository) : IGetMarketplacePluginInstallationStatusService
{
    public async Task<GetMarketplacePluginInstallationStatusResponse> GetStatusAsync(string pluginId, CancellationToken cancellationToken = default)
    {
        var installedPlugins = await repository.GetInstalledPluginsAsync(cancellationToken);
        var match = installedPlugins.FirstOrDefault(p => p.Id == pluginId);
        var isInstalled = match != null;

        return new GetMarketplacePluginInstallationStatusResponse
        {
            PluginId = pluginId,
            IsInstalled = isInstalled,
            InstalledVersion = isInstalled ? match!.Version : null,
            InstalledAt = isInstalled ? match!.InstalledAt : null
        };
    }
}
