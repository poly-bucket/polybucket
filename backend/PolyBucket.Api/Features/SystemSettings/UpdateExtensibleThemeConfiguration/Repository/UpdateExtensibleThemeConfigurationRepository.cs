using PolyBucket.Api.Common.Services;

namespace PolyBucket.Api.Features.SystemSettings.UpdateExtensibleThemeConfiguration.Repository;

public class UpdateExtensibleThemeConfigurationRepository(IThemeManager themeManager) : IUpdateExtensibleThemeConfigurationRepository
{
    public IThemeManager ThemeManager { get; } = themeManager;
}
