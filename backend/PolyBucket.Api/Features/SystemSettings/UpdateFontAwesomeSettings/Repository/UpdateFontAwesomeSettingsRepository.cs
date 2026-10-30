using PolyBucket.Api.Features.SystemSettings.Domain;
using SharedRepo = PolyBucket.Api.Features.SystemSettings.Repository;

namespace PolyBucket.Api.Features.SystemSettings.UpdateFontAwesomeSettings.Repository;

public class UpdateFontAwesomeSettingsRepository(SharedRepo.IFontAwesomeSettingsRepository repository) : IUpdateFontAwesomeSettingsRepository
{
    public async Task<FontAwesomeSettings> UpdateAsync(FontAwesomeSettings settings, CancellationToken cancellationToken = default)
    {
        var existing = await repository.GetSettingsAsync();
        if (existing == null)
        {
            await repository.CreateSettingsAsync(settings);
        }
        else
        {
            existing.IsProEnabled = settings.IsProEnabled;
            existing.ProLicenseKey = settings.ProLicenseKey;
            existing.ProKitUrl = settings.ProKitUrl;
            existing.UseProIcons = settings.UseProIcons;
            existing.FallbackToFree = settings.FallbackToFree;
            await repository.UpdateSettingsAsync(existing);
        }
        return (await repository.GetSettingsAsync()) ?? settings;
    }
}
