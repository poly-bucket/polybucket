using PolyBucket.Api.Common.Plugins;
using PolyBucket.Api.Features.SystemSettings.GetActiveExtensibleThemeComponents.Repository;

namespace PolyBucket.Api.Features.SystemSettings.GetActiveExtensibleThemeComponents.Domain;

public class GetActiveExtensibleThemeComponentsService(IGetActiveExtensibleThemeComponentsRepository repository) : IGetActiveExtensibleThemeComponentsService
{
    public async Task<ThemeComponents?> GetActiveExtensibleThemeComponentsAsync(CancellationToken cancellationToken = default)
    {
        return await repository.ThemeManager.GetActiveThemeComponentsAsync();
    }
}
