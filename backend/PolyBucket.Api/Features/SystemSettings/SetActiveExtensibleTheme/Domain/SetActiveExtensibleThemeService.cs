using PolyBucket.Api.Common.Plugins;
using PolyBucket.Api.Features.SystemSettings.SetActiveExtensibleTheme.Repository;

namespace PolyBucket.Api.Features.SystemSettings.SetActiveExtensibleTheme.Domain;

public class SetActiveExtensibleThemeService(ISetActiveExtensibleThemeRepository repository) : ISetActiveExtensibleThemeService
{
    public async Task<bool> SetActiveExtensibleThemeAsync(string themeId, CancellationToken cancellationToken = default)
    {
        return await repository.ThemeManager.SetActiveThemeAsync(themeId);
    }
}
