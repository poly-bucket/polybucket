using PolyBucket.Storage;
using PolyBucket.Storage.UpdateStorageSettings;

namespace PolyBucket.Api.Features.SystemSettings.UpdateStorageSettings.Domain;

public interface IUpdateStorageSettingsService
{
    Task<StorageSettingsDto> UpdateStorageSettingsAsync(StorageSettingsUpdate update, CancellationToken cancellationToken = default);
}
