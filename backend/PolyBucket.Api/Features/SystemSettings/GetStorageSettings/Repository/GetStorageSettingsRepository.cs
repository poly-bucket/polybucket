using PolyBucket.Storage;
using StorageGet = PolyBucket.Storage.GetStorageSettings.IGetStorageSettingsService;

namespace PolyBucket.Api.Features.SystemSettings.GetStorageSettings.Repository;

public class GetStorageSettingsRepository(StorageGet storageService) : IGetStorageSettingsRepository
{
    public Task<StorageSettingsDto> GetAsync(CancellationToken cancellationToken) =>
        storageService.GetAsync(cancellationToken);
}
