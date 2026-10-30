namespace PolyBucket.Api.Features.Plugins.GetInstalledPluginAsset.Repository;

public interface IGetInstalledPluginAssetRepository
{
    bool TryResolveAssetPath(string pluginId, string assetPath, out string fullPath);
}
