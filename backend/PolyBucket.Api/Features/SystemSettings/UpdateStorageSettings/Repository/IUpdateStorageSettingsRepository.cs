using PolyBucket.Storage;
using PolyBucket.Storage.UpdateStorageSettings;

namespace PolyBucket.Api.Features.SystemSettings.UpdateStorageSettings.Repository;

public interface IUpdateStorageSettingsRepository
{
    Task<StorageSettingsDto> UpdateAsync(StorageSettingsUpdate update, CancellationToken cancellationToken);
}
