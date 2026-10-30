using PolyBucket.Api.Features.Plugins.ApplyTheme.Domain;
using PolyBucket.Api.Features.Plugins.Domain;

namespace PolyBucket.Api.Features.Plugins.GetThemeSettings.Domain;

public class GetThemeSettingsService : IGetThemeSettingsService
{
    public ThemeSimpleOutcome<ThemeSettings> GetSettings(string pluginId)
    {
        if (string.IsNullOrWhiteSpace(pluginId))
            return new() { Status = ThemeRequestStatus.BadRequest, ErrorBody = new { message = "Plugin ID is required" } };
        return new()
        {
            Status = ThemeRequestStatus.Ok,
            Value = new ThemeSettings
            {
                PrimaryColor = "#007bff",
                SecondaryColor = "#6c757d",
                BackgroundColor = "#ffffff",
                SurfaceColor = "#f8f9fa",
                TextColor = "#212529",
                TextMutedColor = "#6c757d",
                BorderColor = "#dee2e6",
                ShadowColor = "rgba(0, 0, 0, 0.1)",
                EnableAnimations = true,
                FontFamily = "system-ui, -apple-system, sans-serif",
                BorderRadius = 4
            }
        };
    }
}
