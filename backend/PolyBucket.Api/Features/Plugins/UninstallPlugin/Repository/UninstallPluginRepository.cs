using PolyBucket.Api.Features.Plugins.Services;

namespace PolyBucket.Api.Features.Plugins.UninstallPlugin.Repository;

public class UninstallPluginRepository(PluginInstallationService installationService) : IUninstallPluginRepository
{
    private readonly PluginInstallationService _installationService = installationService;

    public Task<bool> UninstallPluginAsync(string pluginId, CancellationToken cancellationToken = default) =>
        _installationService.UninstallPluginAsync(pluginId);
}
