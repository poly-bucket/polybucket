using PolyBucket.Api.Common.Plugins;
using PolyBucket.Api.Features.SystemSettings.GetExtensibleThemeComponents.Repository;

namespace PolyBucket.Api.Features.SystemSettings.GetExtensibleThemeComponents.Domain;

public class GetExtensibleThemeComponentsService(IGetExtensibleThemeComponentsRepository repository) : IGetExtensibleThemeComponentsService
{
    public async Task<ThemeComponents?> GetExtensibleThemeComponentsAsync(string themeId, CancellationToken cancellationToken = default)
    {
        return await repository.ThemeManager.GetThemeComponentsAsync(themeId);
    }
}
