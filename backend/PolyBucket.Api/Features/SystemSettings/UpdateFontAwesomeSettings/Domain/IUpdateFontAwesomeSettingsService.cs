using PolyBucket.Api.Features.SystemSettings.Domain;
using PolyBucket.Api.Features.SystemSettings.GetFontAwesomeSettings.Http;

namespace PolyBucket.Api.Features.SystemSettings.UpdateFontAwesomeSettings.Domain;

public interface IUpdateFontAwesomeSettingsService
{
    Task<FontAwesomeSettings> UpdateFontAwesomeSettingsAsync(UpdateFontAwesomeSettingsRequestDto request, CancellationToken cancellationToken = default);
}
