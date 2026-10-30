using PolyBucket.Api.Common.Plugins;

namespace PolyBucket.Api.Features.SystemSettings.SetActiveExtensibleTheme.Domain;

public interface ISetActiveExtensibleThemeService
{
    Task<bool> SetActiveExtensibleThemeAsync(string themeId, CancellationToken cancellationToken = default);
}
