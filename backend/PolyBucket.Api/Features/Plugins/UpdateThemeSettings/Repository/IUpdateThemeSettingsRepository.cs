using PolyBucket.Api.Features.Plugins.Domain;

namespace PolyBucket.Api.Features.Plugins.UpdateThemeSettings.Repository;

public interface IUpdateThemeSettingsRepository
{
    Task<ThemeApplicationResult> UpdateThemeSettingsAsync(string pluginId, ThemeSettings settings);
}
