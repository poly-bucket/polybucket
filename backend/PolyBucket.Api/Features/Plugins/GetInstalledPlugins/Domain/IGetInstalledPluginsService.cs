using PolyBucket.Api.Features.Plugins.Domain;

namespace PolyBucket.Api.Features.Plugins.GetInstalledPlugins.Domain;

public interface IGetInstalledPluginsService
{
    Task<List<InstalledPlugin>> GetInstalledPluginsAsync(CancellationToken cancellationToken = default);
}
