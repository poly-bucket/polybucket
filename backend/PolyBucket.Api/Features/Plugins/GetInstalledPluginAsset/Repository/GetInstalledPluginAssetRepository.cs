using PolyBucket.Api.Features.Plugins.Services;

namespace PolyBucket.Api.Features.Plugins.GetInstalledPluginAsset.Repository;

public class GetInstalledPluginAssetRepository(PluginInstallationService installationService) : IGetInstalledPluginAssetRepository
{
    public bool TryResolveAssetPath(string pluginId, string assetPath, out string fullPath) =>
        installationService.TryResolveAssetPath(pluginId, assetPath, out fullPath);
}
