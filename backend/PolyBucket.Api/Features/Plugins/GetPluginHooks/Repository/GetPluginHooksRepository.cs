using PolyBucket.Api.Common.Plugins;

namespace PolyBucket.Api.Features.Plugins.GetPluginHooks.Repository;

public class GetPluginHooksRepository(PluginManager pluginManager) : IGetPluginHooksRepository
{
    public IReadOnlyList<IPlugin> GetLoadedPlugins() => pluginManager.GetLoadedPlugins().ToList();
}
