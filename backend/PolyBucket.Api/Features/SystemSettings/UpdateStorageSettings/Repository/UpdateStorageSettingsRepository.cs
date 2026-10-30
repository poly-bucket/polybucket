using PolyBucket.Storage;
using PolyBucket.Storage.UpdateStorageSettings;
using StorageUpdate = PolyBucket.Storage.UpdateStorageSettings.IUpdateStorageSettingsService;

namespace PolyBucket.Api.Features.SystemSettings.UpdateStorageSettings.Repository;

public class UpdateStorageSettingsRepository(StorageUpdate storageService) : IUpdateStorageSettingsRepository
{
    public Task<StorageSettingsDto> UpdateAsync(StorageSettingsUpdate update, CancellationToken cancellationToken) =>
        storageService.UpdateAsync(update, cancellationToken);
}
