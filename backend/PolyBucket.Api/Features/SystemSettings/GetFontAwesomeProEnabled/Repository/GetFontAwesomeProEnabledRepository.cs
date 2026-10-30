using PolyBucket.Api.Features.SystemSettings.Domain;
using SharedRepo = PolyBucket.Api.Features.SystemSettings.Repository;

namespace PolyBucket.Api.Features.SystemSettings.GetFontAwesomeProEnabled.Repository;

public class GetFontAwesomeProEnabledRepository(SharedRepo.IFontAwesomeSettingsRepository repository) : IGetFontAwesomeProEnabledRepository
{
    public Task<FontAwesomeSettings?> GetSettingsAsync(CancellationToken cancellationToken = default) =>
        repository.GetSettingsAsync();
}
