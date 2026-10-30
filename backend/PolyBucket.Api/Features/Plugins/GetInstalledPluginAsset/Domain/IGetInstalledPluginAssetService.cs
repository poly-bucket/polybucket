namespace PolyBucket.Api.Features.Plugins.GetInstalledPluginAsset.Domain;

public sealed class InstalledPluginAssetResult
{
    public bool Found { get; init; }
    public string FullPath { get; init; } = string.Empty;
    public string ContentType { get; init; } = "application/octet-stream";
}

public interface IGetInstalledPluginAssetService
{
    InstalledPluginAssetResult ResolveAsset(string pluginId, string assetPath);
}
