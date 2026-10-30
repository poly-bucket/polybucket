using PolyBucket.Api.Features.Plugins.GetInstalledPluginAsset.Repository;

namespace PolyBucket.Api.Features.Plugins.GetInstalledPluginAsset.Domain;

public class GetInstalledPluginAssetService(IGetInstalledPluginAssetRepository repository) : IGetInstalledPluginAssetService
{
    public InstalledPluginAssetResult ResolveAsset(string pluginId, string assetPath)
    {
        if (!repository.TryResolveAssetPath(pluginId, assetPath, out var fullPath))
        {
            return new InstalledPluginAssetResult { Found = false };
        }

        return new InstalledPluginAssetResult
        {
            Found = true,
            FullPath = fullPath,
            ContentType = GetContentType(fullPath)
        };
    }

    private static string GetContentType(string path)
    {
        var extension = Path.GetExtension(path).ToLowerInvariant();
        return extension switch
        {
            ".css" => "text/css",
            ".js" => "application/javascript",
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            ".svg" => "image/svg+xml",
            _ => "application/octet-stream"
        };
    }
}
