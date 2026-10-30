using PolyBucket.Api.Features.SystemSettings.GetFontAwesomeProEnabled.Repository;

namespace PolyBucket.Api.Features.SystemSettings.GetFontAwesomeProEnabled.Domain;

public class GetFontAwesomeProEnabledService(IGetFontAwesomeProEnabledRepository repository) : IGetFontAwesomeProEnabledService
{
    public async Task<bool> IsFontAwesomeProEnabledAsync(CancellationToken cancellationToken = default)
    {
        var settings = await repository.GetSettingsAsync(cancellationToken);
        return settings is { IsProEnabled: true, IsLicenseValid: true };
    }
}
