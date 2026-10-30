using PolyBucket.Api.Common.Plugins;
using PolyBucket.Api.Features.Plugins.UpdatePluginSettings.Http;
using PolyBucket.Api.Features.Plugins.UpdatePluginSettings.Repository;

namespace PolyBucket.Api.Features.Plugins.UpdatePluginSettings.Domain;

public class UpdatePluginSettingsService(IUpdatePluginSettingsRepository repository) : IUpdatePluginSettingsService
{
    public UpdatePluginSettingsResponse? UpdateSettings(string pluginId, UpdatePluginSettingsRequest request)
    {
        var plugin = repository.FindPlugin(pluginId);
        if (plugin == null)
        {
            return null;
        }

        var validationResult = ValidateSettings(plugin, request.Settings);
        if (!validationResult.IsValid)
        {
            return new UpdatePluginSettingsResponse
            {
                Success = false,
                Message = "Settings validation failed",
                ValidationErrors = validationResult.Errors
            };
        }

        return new UpdatePluginSettingsResponse
        {
            Success = true,
            Message = "Plugin settings updated successfully",
            UpdatedSettings = request.Settings,
            UpdatedAt = DateTime.UtcNow
        };
    }

    private static SettingsValidationResult ValidateSettings(IPlugin plugin, Dictionary<string, object> settings)
    {
        var result = new SettingsValidationResult { IsValid = true };
        var pluginSettings = plugin.Metadata?.Settings ?? new Dictionary<string, PluginSetting>();

        foreach (var setting in settings)
        {
            if (!pluginSettings.ContainsKey(setting.Key))
            {
                result.IsValid = false;
                result.Errors.Add($"Unknown setting: {setting.Key}");
                continue;
            }

            var pluginSetting = pluginSettings[setting.Key];
            if (pluginSetting.Required && (setting.Value == null || string.IsNullOrEmpty(setting.Value.ToString())))
            {
                result.IsValid = false;
                result.Errors.Add($"Required setting '{setting.Key}' is missing or empty");
            }
        }

        foreach (var pluginSetting in pluginSettings.Where(ps => ps.Value.Required))
        {
            if (!settings.ContainsKey(pluginSetting.Key))
            {
                result.IsValid = false;
                result.Errors.Add($"Required setting '{pluginSetting.Key}' is missing");
            }
        }

        return result;
    }

    private sealed class SettingsValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new();
    }
}
