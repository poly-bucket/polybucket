using PolyBucket.Api.Common.Plugins;
using PolyBucket.Api.Features.SystemSettings.UpdateExtensibleThemeConfiguration.Repository;

namespace PolyBucket.Api.Features.SystemSettings.UpdateExtensibleThemeConfiguration.Domain;

public class UpdateExtensibleThemeConfigurationService(IUpdateExtensibleThemeConfigurationRepository repository) : IUpdateExtensibleThemeConfigurationService
{
    public async Task<ThemeApplicationResult> UpdateExtensibleThemeConfigurationAsync(Dictionary<string, object> configuration, CancellationToken cancellationToken = default)
    {
        return await repository.ThemeManager.UpdateThemeConfigurationAsync(configuration);
    }
}
