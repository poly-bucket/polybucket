using PolyBucket.Api.Common.Plugins;

namespace PolyBucket.Api.Features.SystemSettings.GetActiveExtensibleTheme.Domain;

public interface IGetActiveExtensibleThemeService
{
    Task<ThemeDefinition?> GetActiveExtensibleThemeAsync(CancellationToken cancellationToken = default);
}
