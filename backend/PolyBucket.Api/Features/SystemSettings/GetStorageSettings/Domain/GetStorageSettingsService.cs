using PolyBucket.Api.Features.SystemSettings.GetStorageSettings.Repository;
using PolyBucket.Storage;

namespace PolyBucket.Api.Features.SystemSettings.GetStorageSettings.Domain;

public class GetStorageSettingsService(IGetStorageSettingsRepository repository) : IGetStorageSettingsService
{
    public Task<StorageSettingsDto> GetStorageSettingsAsync(CancellationToken cancellationToken = default) =>
        repository.GetAsync(cancellationToken);
}
