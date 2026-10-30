using PolyBucket.Api.Common.Plugins;

namespace PolyBucket.Api.Features.SystemSettings.ValidateExtensibleThemeConfiguration.Domain;

public interface IValidateExtensibleThemeConfigurationService
{
    Task<ThemeValidationResult> ValidateExtensibleThemeConfigurationAsync(string themeId, Dictionary<string, object> configuration, CancellationToken cancellationToken = default);
}
