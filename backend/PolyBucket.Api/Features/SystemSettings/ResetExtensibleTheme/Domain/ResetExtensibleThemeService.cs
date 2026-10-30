using PolyBucket.Api.Common.Plugins;
using PolyBucket.Api.Features.SystemSettings.ResetExtensibleTheme.Repository;

namespace PolyBucket.Api.Features.SystemSettings.ResetExtensibleTheme.Domain;

public class ResetExtensibleThemeService(IResetExtensibleThemeRepository repository) : IResetExtensibleThemeService
{
    public async Task<ThemeApplicationResult> ResetExtensibleThemeAsync(CancellationToken cancellationToken = default)
    {
        return await repository.ThemeManager.ResetThemeToDefaultAsync();
    }
}
