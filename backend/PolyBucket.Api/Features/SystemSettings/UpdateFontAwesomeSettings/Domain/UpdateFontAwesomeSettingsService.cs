using PolyBucket.Api.Features.SystemSettings.Domain;
using PolyBucket.Api.Features.SystemSettings.GetFontAwesomeSettings.Http;
using PolyBucket.Api.Features.SystemSettings.GetFontAwesomeSettings.Repository;
using PolyBucket.Api.Features.SystemSettings.UpdateFontAwesomeSettings.Repository;

namespace PolyBucket.Api.Features.SystemSettings.UpdateFontAwesomeSettings.Domain;

public class UpdateFontAwesomeSettingsService(
    IGetFontAwesomeSettingsRepository getRepository,
    IUpdateFontAwesomeSettingsRepository updateRepository) : IUpdateFontAwesomeSettingsService
{
    public async Task<FontAwesomeSettings> UpdateFontAwesomeSettingsAsync(UpdateFontAwesomeSettingsRequestDto request, CancellationToken cancellationToken = default)
    {
        var current = await getRepository.GetOrCreateAsync(cancellationToken);
        var entity = request.ToEntity(current.Id);
        if (string.IsNullOrEmpty(request.ProLicenseKey))
        {
            entity.ProLicenseKey = current.ProLicenseKey;
        }
        return await updateRepository.UpdateAsync(entity, cancellationToken);
    }
}
