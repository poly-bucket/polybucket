using PolyBucket.Api.Features.Plugins.Domain;
using PolyBucket.Api.Features.Plugins.Services;

namespace PolyBucket.Api.Features.Plugins.InstallPlugin.Repository;

public class InstallPluginRepository(
    PluginInstallationService installationService,
    MarketplaceClient marketplaceClient) : IInstallPluginRepository
{
    private readonly PluginInstallationService _installationService = installationService;
    private readonly MarketplaceClient _marketplaceClient = marketplaceClient;

    public Task<PluginInstallationResult> InstallFromUrlAsync(string url, CancellationToken cancellationToken = default) =>
        _installationService.InstallFromUrlAsync(url);

    public Task<PluginInstallationResult> InstallFromGitHubAsync(string url, string? branch, CancellationToken cancellationToken = default) =>
        _installationService.InstallFromGitHubAsync(url, branch);

    public Task<MarketplacePluginDetails?> GetMarketplacePluginDetailsAsync(string pluginId, CancellationToken cancellationToken = default) =>
        _marketplaceClient.GetPluginDetailsAsync(pluginId);

    public Task<PluginInstallationResult> InstallFromMarketplaceAsync(MarketplacePluginDetails listing, CancellationToken cancellationToken = default) =>
        _installationService.InstallFromMarketplaceAsync(listing);
}
