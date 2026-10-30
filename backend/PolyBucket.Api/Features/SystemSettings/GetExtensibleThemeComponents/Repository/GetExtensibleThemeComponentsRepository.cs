using PolyBucket.Api.Common.Services;

namespace PolyBucket.Api.Features.SystemSettings.GetExtensibleThemeComponents.Repository;

public class GetExtensibleThemeComponentsRepository(IThemeManager themeManager) : IGetExtensibleThemeComponentsRepository
{
    public IThemeManager ThemeManager { get; } = themeManager;
}
