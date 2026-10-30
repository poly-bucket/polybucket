using PolyBucket.Api.Common.Plugins;
using PolyBucket.Api.Features.SystemSettings.ValidateExtensibleThemeConfiguration.Repository;

namespace PolyBucket.Api.Features.SystemSettings.ValidateExtensibleThemeConfiguration.Domain;

public class ValidateExtensibleThemeConfigurationService(IValidateExtensibleThemeConfigurationRepository repository) : IValidateExtensibleThemeConfigurationService
{
    public async Task<ThemeValidationResult> ValidateExtensibleThemeConfigurationAsync(string themeId, Dictionary<string, object> configuration, CancellationToken cancellationToken = default)
    {
        return await repository.ThemeManager.ValidateThemeConfigurationAsync(themeId, configuration);
    }
}
