using PolyBucket.Storage;

namespace PolyBucket.Api.Features.SystemSettings.GetStorageSettings.Domain;

public interface IGetStorageSettingsService
{
    Task<StorageSettingsDto> GetStorageSettingsAsync(CancellationToken cancellationToken = default);
}
