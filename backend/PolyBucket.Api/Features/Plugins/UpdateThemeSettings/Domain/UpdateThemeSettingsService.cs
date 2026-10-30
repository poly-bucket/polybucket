using PolyBucket.Api.Features.Plugins.ApplyTheme.Domain;
using PolyBucket.Api.Features.Plugins.Domain;
using PolyBucket.Api.Features.Plugins.UpdateThemeSettings.Repository;

namespace PolyBucket.Api.Features.Plugins.UpdateThemeSettings.Domain;

public class UpdateThemeSettingsService(IUpdateThemeSettingsRepository repository) : IUpdateThemeSettingsService
{
    public async Task<ThemeSimpleOutcome<ThemeApplicationResult>> UpdateAsync(string pluginId, ThemeSettings settings)
    {
        if (string.IsNullOrWhiteSpace(pluginId))
            return new() { Status = ThemeRequestStatus.BadRequest, ErrorBody = new { message = "Plugin ID is required" } };
        if (settings == null)
            return new() { Status = ThemeRequestStatus.BadRequest, ErrorBody = new { message = "Theme settings are required" } };
        var result = await repository.UpdateThemeSettingsAsync(pluginId, settings);
        return result.Success
            ? new() { Status = ThemeRequestStatus.Ok, Value = result }
            : new() { Status = ThemeRequestStatus.BadRequest, Value = result };
    }
}
