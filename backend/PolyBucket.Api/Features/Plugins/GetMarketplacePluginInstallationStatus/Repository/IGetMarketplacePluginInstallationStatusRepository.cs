using PolyBucket.Api.Features.Plugins.Domain;

namespace PolyBucket.Api.Features.Plugins.GetMarketplacePluginInstallationStatus.Repository;

public interface IGetMarketplacePluginInstallationStatusRepository
{
    Task<List<InstalledPlugin>> GetInstalledPluginsAsync(CancellationToken cancellationToken = default);
}
