namespace PolyBucket.Api.Features.Plugins.GetMarketplacePluginInstallationStatus.Domain;

public sealed class GetMarketplacePluginInstallationStatusResponse
{
    public string PluginId { get; init; } = string.Empty;
    public bool IsInstalled { get; init; }
    public string? InstalledVersion { get; init; }
    public DateTime? InstalledAt { get; init; }
}
