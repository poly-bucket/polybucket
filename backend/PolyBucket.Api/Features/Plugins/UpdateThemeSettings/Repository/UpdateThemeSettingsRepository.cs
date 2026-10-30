using PolyBucket.Api.Features.Plugins.Domain;
using PolyBucket.Api.Features.Plugins.Services;

namespace PolyBucket.Api.Features.Plugins.UpdateThemeSettings.Repository;

public class UpdateThemeSettingsRepository(ThemePluginService themePluginService) : IUpdateThemeSettingsRepository
{
    public Task<ThemeApplicationResult> UpdateThemeSettingsAsync(string pluginId, ThemeSettings settings) =>
        themePluginService.UpdateThemeSettingsAsync(pluginId, settings);
}
