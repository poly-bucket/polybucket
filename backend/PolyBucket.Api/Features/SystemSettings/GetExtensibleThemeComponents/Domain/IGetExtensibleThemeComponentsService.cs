using PolyBucket.Api.Common.Plugins;

namespace PolyBucket.Api.Features.SystemSettings.GetExtensibleThemeComponents.Domain;

public interface IGetExtensibleThemeComponentsService
{
    Task<ThemeComponents?> GetExtensibleThemeComponentsAsync(string themeId, CancellationToken cancellationToken = default);
}
