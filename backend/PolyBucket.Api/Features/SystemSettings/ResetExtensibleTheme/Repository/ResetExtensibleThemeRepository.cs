using PolyBucket.Api.Common.Services;

namespace PolyBucket.Api.Features.SystemSettings.ResetExtensibleTheme.Repository;

public class ResetExtensibleThemeRepository(IThemeManager themeManager) : IResetExtensibleThemeRepository
{
    public IThemeManager ThemeManager { get; } = themeManager;
}
