using PolyBucket.Api.Features.Plugins.ApplyTheme.Domain;
using PolyBucket.Api.Features.Plugins.Domain;

namespace PolyBucket.Api.Features.Plugins.GetThemeSettings.Domain;

public interface IGetThemeSettingsService
{
    ThemeSimpleOutcome<ThemeSettings> GetSettings(string pluginId);
}
