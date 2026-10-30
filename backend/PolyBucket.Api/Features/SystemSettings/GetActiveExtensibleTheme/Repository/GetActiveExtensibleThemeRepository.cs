using PolyBucket.Api.Common.Services;

namespace PolyBucket.Api.Features.SystemSettings.GetActiveExtensibleTheme.Repository;

public class GetActiveExtensibleThemeRepository(IThemeManager themeManager) : IGetActiveExtensibleThemeRepository
{
    public IThemeManager ThemeManager { get; } = themeManager;
}
