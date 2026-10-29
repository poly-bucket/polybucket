namespace PolyBucket.Storage.UpdateStorageSettings;

public interface IUpdateStorageSettingsService
{
    Task<StorageSettingsDto> UpdateAsync(StorageSettingsUpdate update, CancellationToken cancellationToken = default);
}
