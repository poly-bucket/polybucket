using PolyBucket.Api.Common.Services;

namespace PolyBucket.Api.Features.SystemSettings.GetExtensibleThemeConfiguration.Repository;

public class GetExtensibleThemeConfigurationRepository(IThemeManager themeManager) : IGetExtensibleThemeConfigurationRepository
{
    public IThemeManager ThemeManager { get; } = themeManager;
}
