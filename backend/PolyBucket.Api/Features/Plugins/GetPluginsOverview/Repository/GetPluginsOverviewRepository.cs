using PolyBucket.Api.Common.Plugins;

namespace PolyBucket.Api.Features.Plugins.GetPluginsOverview.Repository;

public class GetPluginsOverviewRepository(PluginManager pluginManager) : IGetPluginsOverviewRepository
{
    public IReadOnlyList<IPlugin> GetLoadedPlugins() => pluginManager.GetLoadedPlugins().ToList();
}
