using PolyBucket.Api.Common.Plugins;

namespace PolyBucket.Api.Features.SystemSettings.GetExtensibleThemePlugins.Domain;

public interface IGetExtensibleThemePluginsService
{
    Task<IReadOnlyList<IThemePlugin>> GetExtensibleThemePluginsAsync(CancellationToken cancellationToken = default);
}
