using PolyBucket.Api.Features.Plugins.Domain;

namespace PolyBucket.Api.Features.Plugins.ApplyTheme.Domain;

public class ApplyThemeService : IApplyThemeService
{
    public ThemeSimpleOutcome<ThemeApplicationResult> Apply(string pluginId)
    {
        if (string.IsNullOrWhiteSpace(pluginId))
            return new() { Status = ThemeRequestStatus.BadRequest, ErrorBody = new { message = "Plugin ID is required" } };
        return new()
        {
            Status = ThemeRequestStatus.Ok,
            Value = new ThemeApplicationResult
            {
                Success = true,
                Message = $"Theme from plugin {pluginId} applied successfully",
                AppliedVariables = new Dictionary<string, string>
                {
                    ["--primary-color"] = "#007bff",
                    ["--background-color"] = "#ffffff"
                }
            }
        };
    }
}
