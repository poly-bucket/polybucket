using PolyBucket.Api.Features.Plugins.Domain;

namespace PolyBucket.Api.Features.Plugins.GetInstalledPlugins.Repository;

public interface IGetInstalledPluginsRepository
{
    Task<List<InstalledPlugin>> GetInstalledPluginsAsync(CancellationToken cancellationToken = default);
}
