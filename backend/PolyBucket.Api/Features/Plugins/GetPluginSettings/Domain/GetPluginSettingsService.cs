using PolyBucket.Api.Features.Plugins.GetPluginSettings.Http;
using PolyBucket.Api.Features.Plugins.GetPluginSettings.Repository;

namespace PolyBucket.Api.Features.Plugins.GetPluginSettings.Domain;

public class GetPluginSettingsService(IGetPluginSettingsRepository repository) : IGetPluginSettingsService
{
    public PluginSettingsResponse? GetSettings(string pluginId)
    {
        var plugin = repository.FindPlugin(pluginId);
        if (plugin == null)
        {
            return null;
        }

        return new PluginSettingsResponse
        {
            PluginId = plugin.Id,
            PluginName = plugin.Name,
            Settings = plugin.Metadata?.Settings ?? new Dictionary<string, Common.Plugins.PluginSetting>(),
            CurrentValues = new Dictionary<string, object>(),
            Schema = plugin.Metadata?.Settings?.ToDictionary(
                kvp => kvp.Key,
                kvp => new PluginSettingSchema
                {
                    Type = kvp.Value.Type.ToString(),
                    Description = kvp.Value.Description,
                    Required = kvp.Value.Required,
                    DefaultValue = kvp.Value.DefaultValue,
                    Options = kvp.Value.Options
                }) ?? new Dictionary<string, PluginSettingSchema>()
        };
    }
}
