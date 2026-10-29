namespace PolyBucket.Storage.GetStorageSettings;

public class GetStorageSettingsService(IStorageSettingsResolver settingsResolver) : IGetStorageSettingsService
{
    public async Task<StorageSettingsDto> GetAsync(CancellationToken cancellationToken = default)
    {
        var settings = await settingsResolver.GetEffectiveSettingsAsync(cancellationToken);
        return StorageSettingsDto.From(settings);
    }
}
