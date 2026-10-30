using PolyBucket.Api.Features.Plugins.Domain;
using PolyBucket.Api.Features.Plugins.Services;

namespace PolyBucket.Api.Features.Plugins.GetPluginHealth.Repository;

public class GetPluginHealthRepository(PluginInstallationService installationService) : IGetPluginHealthRepository
{
    public Task<List<InstalledPlugin>> GetInstalledPluginsAsync(CancellationToken cancellationToken = default) =>
        installationService.GetInstalledPluginsAsync();
}
