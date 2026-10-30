using PolyBucket.Api.Common.Services;

namespace PolyBucket.Api.Features.SystemSettings.GetExtensibleThemePlugins.Repository;

public class GetExtensibleThemePluginsRepository(IThemeManager themeManager) : IGetExtensibleThemePluginsRepository
{
    public IThemeManager ThemeManager { get; } = themeManager;
}
