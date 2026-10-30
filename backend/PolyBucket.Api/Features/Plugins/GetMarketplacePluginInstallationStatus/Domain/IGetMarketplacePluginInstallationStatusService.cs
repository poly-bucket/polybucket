namespace PolyBucket.Api.Features.Plugins.GetMarketplacePluginInstallationStatus.Domain;

public interface IGetMarketplacePluginInstallationStatusService
{
    Task<GetMarketplacePluginInstallationStatusResponse> GetStatusAsync(string pluginId, CancellationToken cancellationToken = default);
}
