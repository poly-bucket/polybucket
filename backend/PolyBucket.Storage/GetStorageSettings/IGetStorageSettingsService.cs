namespace PolyBucket.Storage.GetStorageSettings;

public interface IGetStorageSettingsService
{
    Task<StorageSettingsDto> GetAsync(CancellationToken cancellationToken = default);
}
