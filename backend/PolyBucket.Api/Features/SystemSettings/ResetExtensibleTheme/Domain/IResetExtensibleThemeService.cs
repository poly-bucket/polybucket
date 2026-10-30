using PolyBucket.Api.Common.Plugins;

namespace PolyBucket.Api.Features.SystemSettings.ResetExtensibleTheme.Domain;

public interface IResetExtensibleThemeService
{
    Task<ThemeApplicationResult> ResetExtensibleThemeAsync(CancellationToken cancellationToken = default);
}
