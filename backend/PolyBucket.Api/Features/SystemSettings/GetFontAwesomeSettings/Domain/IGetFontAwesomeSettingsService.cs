using PolyBucket.Api.Features.SystemSettings.Domain;

namespace PolyBucket.Api.Features.SystemSettings.GetFontAwesomeSettings.Domain;

public interface IGetFontAwesomeSettingsService
{
    Task<FontAwesomeSettings> GetFontAwesomeSettingsAsync(CancellationToken cancellationToken = default);
}
