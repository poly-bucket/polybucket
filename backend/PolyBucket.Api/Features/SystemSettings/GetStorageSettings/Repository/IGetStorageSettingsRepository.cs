using PolyBucket.Storage;

namespace PolyBucket.Api.Features.SystemSettings.GetStorageSettings.Repository;

public interface IGetStorageSettingsRepository
{
    Task<StorageSettingsDto> GetAsync(CancellationToken cancellationToken);
}
