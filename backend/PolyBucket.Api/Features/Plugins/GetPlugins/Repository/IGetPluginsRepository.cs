using PolyBucket.Api.Features.Plugins.GetPlugins.Domain;

namespace PolyBucket.Api.Features.Plugins.GetPlugins.Repository;

public interface IGetPluginsRepository
{
    IReadOnlyList<GetPluginsItem> GetLoadedPlugins();
}
