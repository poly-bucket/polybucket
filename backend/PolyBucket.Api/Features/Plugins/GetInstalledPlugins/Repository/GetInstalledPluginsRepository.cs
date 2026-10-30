using PolyBucket.Api.Features.Plugins.Domain;
using PolyBucket.Api.Features.Plugins.Services;

namespace PolyBucket.Api.Features.Plugins.GetInstalledPlugins.Repository;

public class GetInstalledPluginsRepository(PluginInstallationService installationService) : IGetInstalledPluginsRepository
{
    private readonly PluginInstallationService _installationService = installationService;

    public Task<List<InstalledPlugin>> GetInstalledPluginsAsync(CancellationToken cancellationToken = default) =>
        _installationService.GetInstalledPluginsAsync();
}
