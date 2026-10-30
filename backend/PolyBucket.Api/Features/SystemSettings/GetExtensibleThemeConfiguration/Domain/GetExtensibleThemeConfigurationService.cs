using PolyBucket.Api.Common.Plugins;
using PolyBucket.Api.Features.SystemSettings.GetExtensibleThemeConfiguration.Repository;

namespace PolyBucket.Api.Features.SystemSettings.GetExtensibleThemeConfiguration.Domain;

public class GetExtensibleThemeConfigurationService(IGetExtensibleThemeConfigurationRepository repository) : IGetExtensibleThemeConfigurationService
{
    public async Task<Dictionary<string, object>> GetExtensibleThemeConfigurationAsync(CancellationToken cancellationToken = default)
    {
        return await repository.ThemeManager.GetCurrentThemeConfigurationAsync();
    }
}
