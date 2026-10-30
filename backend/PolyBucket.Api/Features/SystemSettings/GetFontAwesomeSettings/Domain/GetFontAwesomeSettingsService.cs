using PolyBucket.Api.Features.SystemSettings.Domain;
using PolyBucket.Api.Features.SystemSettings.GetFontAwesomeSettings.Repository;

namespace PolyBucket.Api.Features.SystemSettings.GetFontAwesomeSettings.Domain;

public class GetFontAwesomeSettingsService(IGetFontAwesomeSettingsRepository repository) : IGetFontAwesomeSettingsService
{
    public Task<FontAwesomeSettings> GetFontAwesomeSettingsAsync(CancellationToken cancellationToken = default) =>
        repository.GetOrCreateAsync(cancellationToken);
}
