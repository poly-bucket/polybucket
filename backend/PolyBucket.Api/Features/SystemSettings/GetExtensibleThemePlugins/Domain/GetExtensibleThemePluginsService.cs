using PolyBucket.Api.Common.Plugins;
using PolyBucket.Api.Features.SystemSettings.GetExtensibleThemePlugins.Repository;

namespace PolyBucket.Api.Features.SystemSettings.GetExtensibleThemePlugins.Domain;

public class GetExtensibleThemePluginsService(IGetExtensibleThemePluginsRepository repository) : IGetExtensibleThemePluginsService
{
    public async Task<IReadOnlyList<IThemePlugin>> GetExtensibleThemePluginsAsync(CancellationToken cancellationToken = default)
    {
        return await Task.FromResult<IReadOnlyList<IThemePlugin>>(repository.ThemeManager.GetThemePlugins().ToList());
    }
}
