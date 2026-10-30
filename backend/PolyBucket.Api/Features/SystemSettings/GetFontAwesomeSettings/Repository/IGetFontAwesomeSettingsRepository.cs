using PolyBucket.Api.Features.SystemSettings.Domain;

namespace PolyBucket.Api.Features.SystemSettings.GetFontAwesomeSettings.Repository;

public interface IGetFontAwesomeSettingsRepository
{
    Task<FontAwesomeSettings> GetOrCreateAsync(CancellationToken cancellationToken = default);
}
