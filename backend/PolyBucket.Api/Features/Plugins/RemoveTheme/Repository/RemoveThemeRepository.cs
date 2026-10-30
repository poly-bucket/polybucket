using PolyBucket.Api.Features.Plugins.Domain;
using PolyBucket.Api.Features.Plugins.Services;

namespace PolyBucket.Api.Features.Plugins.RemoveTheme.Repository;

public class RemoveThemeRepository(ThemePluginService themePluginService) : IRemoveThemeRepository
{
    public Task<ThemeApplicationResult> RemoveThemeAsync(string pluginId) => themePluginService.RemoveThemeAsync(pluginId);
}
