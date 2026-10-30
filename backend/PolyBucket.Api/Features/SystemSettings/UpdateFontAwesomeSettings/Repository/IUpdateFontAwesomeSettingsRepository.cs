using PolyBucket.Api.Features.SystemSettings.Domain;

namespace PolyBucket.Api.Features.SystemSettings.UpdateFontAwesomeSettings.Repository;

public interface IUpdateFontAwesomeSettingsRepository
{
    Task<FontAwesomeSettings> UpdateAsync(FontAwesomeSettings settings, CancellationToken cancellationToken = default);
}
