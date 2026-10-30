using PolyBucket.Api.Features.SystemSettings.TestStorageConfiguration.Repository;
using PolyBucket.Storage.TestStorageConfiguration;
using PolyBucket.Storage.UpdateStorageSettings;

namespace PolyBucket.Api.Features.SystemSettings.TestStorageConfiguration.Domain;

public class TestStorageConfigurationService(ITestStorageConfigurationRepository repository) : ITestStorageConfigurationService
{
    public Task<TestStorageConfigurationResult> TestStorageConfigurationAsync(StorageSettingsUpdate? proposed, CancellationToken cancellationToken = default) =>
        repository.TestAsync(proposed, cancellationToken);
}
