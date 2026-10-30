using PolyBucket.Api.Common.Plugins;

namespace PolyBucket.Api.Features.SystemSettings.UpdateExtensibleThemeConfiguration.Domain;

public interface IUpdateExtensibleThemeConfigurationService
{
    Task<ThemeApplicationResult> UpdateExtensibleThemeConfigurationAsync(Dictionary<string, object> configuration, CancellationToken cancellationToken = default);
}
