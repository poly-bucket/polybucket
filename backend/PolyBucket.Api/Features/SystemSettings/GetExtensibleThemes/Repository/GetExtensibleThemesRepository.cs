using PolyBucket.Api.Common.Services;

namespace PolyBucket.Api.Features.SystemSettings.GetExtensibleThemes.Repository;

public class GetExtensibleThemesRepository(IThemeManager themeManager) : IGetExtensibleThemesRepository
{
    public IThemeManager ThemeManager { get; } = themeManager;
}
