using PolyBucket.Api.Features.Plugins.Domain;
using PolyBucket.Api.Features.Plugins.ApplyTheme.Domain;
using PolyBucket.Api.Features.Plugins.RemoveTheme.Repository;

namespace PolyBucket.Api.Features.Plugins.RemoveTheme.Domain;

public class RemoveThemeService(IRemoveThemeRepository repository) : IRemoveThemeService
{
    public async Task<ThemeSimpleOutcome<ThemeApplicationResult>> RemoveAsync(string pluginId)
    {
        if (string.IsNullOrWhiteSpace(pluginId))
            return new() { Status = ThemeRequestStatus.BadRequest, ErrorBody = new { message = "Plugin ID is required" } };
        var result = await repository.RemoveThemeAsync(pluginId);
        return result.Success
            ? new() { Status = ThemeRequestStatus.Ok, Value = result }
            : new() { Status = ThemeRequestStatus.BadRequest, Value = result };
    }
}
