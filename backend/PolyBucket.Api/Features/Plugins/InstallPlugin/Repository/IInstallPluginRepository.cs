using PolyBucket.Api.Features.Plugins.Domain;
using PolyBucket.Api.Features.Plugins.Services;

namespace PolyBucket.Api.Features.Plugins.InstallPlugin.Repository;

public interface IInstallPluginRepository
{
    Task<PluginInstallationResult> InstallFromUrlAsync(string url, CancellationToken cancellationToken = default);

    Task<PluginInstallationResult> InstallFromGitHubAsync(string url, string? branch, CancellationToken cancellationToken = default);

    Task<MarketplacePluginDetails?> GetMarketplacePluginDetailsAsync(string pluginId, CancellationToken cancellationToken = default);

    Task<PluginInstallationResult> InstallFromMarketplaceAsync(MarketplacePluginDetails listing, CancellationToken cancellationToken = default);
}
