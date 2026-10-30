using PolyBucket.Api.Common.Plugins;

namespace PolyBucket.Api.Features.SystemSettings.GetExtensibleThemeConfiguration.Domain;

public interface IGetExtensibleThemeConfigurationService
{
    Task<Dictionary<string, object>> GetExtensibleThemeConfigurationAsync(CancellationToken cancellationToken = default);
}
