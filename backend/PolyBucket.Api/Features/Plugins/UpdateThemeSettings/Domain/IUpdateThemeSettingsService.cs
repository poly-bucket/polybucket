using PolyBucket.Api.Features.Plugins.ApplyTheme.Domain;
using PolyBucket.Api.Features.Plugins.Domain;

namespace PolyBucket.Api.Features.Plugins.UpdateThemeSettings.Domain;

public interface IUpdateThemeSettingsService
{
    Task<ThemeSimpleOutcome<ThemeApplicationResult>> UpdateAsync(string pluginId, ThemeSettings settings);
}
