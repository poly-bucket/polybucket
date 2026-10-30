using PolyBucket.Api.Features.Plugins.GetPluginSettings.Http;

namespace PolyBucket.Api.Features.Plugins.GetPluginSettings.Domain;

public interface IGetPluginSettingsService
{
    PluginSettingsResponse? GetSettings(string pluginId);
}
