using PolyBucket.Api.Features.Plugins.Domain;

namespace PolyBucket.Api.Features.Plugins.GetPluginHealth.Repository;

public interface IGetPluginHealthRepository
{
    Task<List<InstalledPlugin>> GetInstalledPluginsAsync(CancellationToken cancellationToken = default);
}
