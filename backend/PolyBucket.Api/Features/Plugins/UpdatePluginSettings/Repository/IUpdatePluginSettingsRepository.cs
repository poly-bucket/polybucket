using PolyBucket.Api.Common.Plugins;

namespace PolyBucket.Api.Features.Plugins.UpdatePluginSettings.Repository;

public interface IUpdatePluginSettingsRepository
{
    IPlugin? FindPlugin(string pluginId);
}
