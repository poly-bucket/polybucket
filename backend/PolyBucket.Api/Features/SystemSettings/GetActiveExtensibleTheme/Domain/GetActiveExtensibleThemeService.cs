using PolyBucket.Api.Common.Plugins;
using PolyBucket.Api.Features.SystemSettings.GetActiveExtensibleTheme.Repository;

namespace PolyBucket.Api.Features.SystemSettings.GetActiveExtensibleTheme.Domain;

public class GetActiveExtensibleThemeService(IGetActiveExtensibleThemeRepository repository) : IGetActiveExtensibleThemeService
{
    public async Task<ThemeDefinition?> GetActiveExtensibleThemeAsync(CancellationToken cancellationToken = default)
    {
        return await repository.ThemeManager.GetActiveThemeAsync();
    }
}
