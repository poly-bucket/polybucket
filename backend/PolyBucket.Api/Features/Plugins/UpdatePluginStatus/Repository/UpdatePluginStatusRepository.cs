using PolyBucket.Api.Common.Plugins;

namespace PolyBucket.Api.Features.Plugins.UpdatePluginStatus.Repository;

public class UpdatePluginStatusRepository(PluginManager pluginManager) : IUpdatePluginStatusRepository
{
    public IPlugin? FindPlugin(string pluginId) =>
        pluginManager.GetLoadedPlugins().FirstOrDefault(p => p.Id == pluginId);
}
