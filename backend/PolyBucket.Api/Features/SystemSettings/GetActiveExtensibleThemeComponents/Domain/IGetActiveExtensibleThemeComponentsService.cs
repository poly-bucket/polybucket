using PolyBucket.Api.Common.Plugins;

namespace PolyBucket.Api.Features.SystemSettings.GetActiveExtensibleThemeComponents.Domain;

public interface IGetActiveExtensibleThemeComponentsService
{
    Task<ThemeComponents?> GetActiveExtensibleThemeComponentsAsync(CancellationToken cancellationToken = default);
}
