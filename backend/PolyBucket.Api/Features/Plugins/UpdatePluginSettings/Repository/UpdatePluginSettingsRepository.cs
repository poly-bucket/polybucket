using PolyBucket.Api.Common.Plugins;

namespace PolyBucket.Api.Features.Plugins.UpdatePluginSettings.Repository;

public class UpdatePluginSettingsRepository(PluginManager pluginManager) : IUpdatePluginSettingsRepository
{
    public IPlugin? FindPlugin(string pluginId) =>
        pluginManager.GetLoadedPlugins().FirstOrDefault(p => p.Id == pluginId);
}
