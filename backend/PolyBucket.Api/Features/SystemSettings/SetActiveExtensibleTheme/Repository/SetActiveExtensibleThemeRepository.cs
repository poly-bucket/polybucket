using PolyBucket.Api.Common.Services;

namespace PolyBucket.Api.Features.SystemSettings.SetActiveExtensibleTheme.Repository;

public class SetActiveExtensibleThemeRepository(IThemeManager themeManager) : ISetActiveExtensibleThemeRepository
{
    public IThemeManager ThemeManager { get; } = themeManager;
}
