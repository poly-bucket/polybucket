using PolyBucket.Api.Common.Plugins;

namespace PolyBucket.Api.Features.Plugins.GetPluginDetails.Repository;

public class GetPluginDetailsRepository(PluginManager pluginManager) : IGetPluginDetailsRepository
{
    public IPlugin? FindPlugin(string pluginId) =>
        pluginManager.GetLoadedPlugins().FirstOrDefault(p => p.Id == pluginId);
}
