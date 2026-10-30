using PolyBucket.Api.Common.Services;

namespace PolyBucket.Api.Features.SystemSettings.ValidateExtensibleThemeConfiguration.Repository;

public class ValidateExtensibleThemeConfigurationRepository(IThemeManager themeManager) : IValidateExtensibleThemeConfigurationRepository
{
    public IThemeManager ThemeManager { get; } = themeManager;
}
