using PolyBucket.Api.Common.Services;

namespace PolyBucket.Api.Features.SystemSettings.GetActiveExtensibleThemeComponents.Repository;

public class GetActiveExtensibleThemeComponentsRepository(IThemeManager themeManager) : IGetActiveExtensibleThemeComponentsRepository
{
    public IThemeManager ThemeManager { get; } = themeManager;
}
