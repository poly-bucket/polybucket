using PolyBucket.Api.Features.Plugins.UpdatePluginSettings.Http;

namespace PolyBucket.Api.Features.Plugins.UpdatePluginSettings.Domain;

public interface IUpdatePluginSettingsService
{
    UpdatePluginSettingsResponse? UpdateSettings(string pluginId, UpdatePluginSettingsRequest request);
}
