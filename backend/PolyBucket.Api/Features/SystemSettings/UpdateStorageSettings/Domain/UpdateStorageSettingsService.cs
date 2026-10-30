using ApiUpdateStorageSettingsRepository = PolyBucket.Api.Features.SystemSettings.UpdateStorageSettings.Repository;

namespace PolyBucket.Api.Features.SystemSettings.UpdateStorageSettings.Domain;

public class UpdateStorageSettingsService(ApiUpdateStorageSettingsRepository.IUpdateStorageSettingsRepository repository) : IUpdateStorageSettingsService
{
    public Task<StorageSettingsDto> UpdateStorageSettingsAsync(StorageSettingsUpdate update, CancellationToken cancellationToken = default) =>
        repository.UpdateAsync(update, cancellationToken);
}
