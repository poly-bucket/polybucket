using PolyBucket.Api.Features.Plugins.Domain;
using PolyBucket.Api.Features.Plugins.Services;

namespace PolyBucket.Api.Features.Plugins.GetMarketplacePluginInstallationStatus.Repository;

public class GetMarketplacePluginInstallationStatusRepository(PluginInstallationService installationService) : IGetMarketplacePluginInstallationStatusRepository
{
    public Task<List<InstalledPlugin>> GetInstalledPluginsAsync(CancellationToken cancellationToken = default) =>
        installationService.GetInstalledPluginsAsync();
}
