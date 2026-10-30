using PolyBucket.Api.Common.Plugins;

namespace PolyBucket.Api.Features.Plugins.GetPluginSettings.Repository;

public interface IGetPluginSettingsRepository
{
    IPlugin? FindPlugin(string pluginId);
}
