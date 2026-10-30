using PolyBucket.Api.Features.SystemSettings.Domain;
using SharedRepo = PolyBucket.Api.Features.SystemSettings.Repository;

namespace PolyBucket.Api.Features.SystemSettings.GetFontAwesomeSettings.Repository;

public class GetFontAwesomeSettingsRepository(SharedRepo.IFontAwesomeSettingsRepository repository) : IGetFontAwesomeSettingsRepository
{
    public async Task<FontAwesomeSettings> GetOrCreateAsync(CancellationToken cancellationToken = default)
    {
        var settings = await repository.GetSettingsAsync();
        if (settings == null)
        {
            settings = new FontAwesomeSettings();
            await repository.CreateSettingsAsync(settings);
        }
        return settings;
    }
}
