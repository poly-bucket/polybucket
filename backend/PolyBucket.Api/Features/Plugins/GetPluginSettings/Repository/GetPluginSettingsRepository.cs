using PolyBucket.Api.Common.Plugins;

namespace PolyBucket.Api.Features.Plugins.GetPluginSettings.Repository;

public class GetPluginSettingsRepository(PluginManager pluginManager) : IGetPluginSettingsRepository
{
    public IPlugin? FindPlugin(string pluginId) =>
        pluginManager.GetLoadedPlugins().FirstOrDefault(p => p.Id == pluginId);
}
