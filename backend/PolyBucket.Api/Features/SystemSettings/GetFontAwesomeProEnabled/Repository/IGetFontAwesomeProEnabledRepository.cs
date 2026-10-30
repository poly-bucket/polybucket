using PolyBucket.Api.Features.SystemSettings.Domain;

namespace PolyBucket.Api.Features.SystemSettings.GetFontAwesomeProEnabled.Repository;

public interface IGetFontAwesomeProEnabledRepository
{
    Task<FontAwesomeSettings?> GetSettingsAsync(CancellationToken cancellationToken = default);
}
