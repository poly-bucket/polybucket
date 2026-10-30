using PolyBucket.Storage.TestStorageConfiguration;
using PolyBucket.Storage.UpdateStorageSettings;
using StorageTest = PolyBucket.Storage.TestStorageConfiguration.ITestStorageConfigurationService;

namespace PolyBucket.Api.Features.SystemSettings.TestStorageConfiguration.Repository;

public class TestStorageConfigurationRepository(StorageTest storageService) : ITestStorageConfigurationRepository
{
    public Task<TestStorageConfigurationResult> TestAsync(StorageSettingsUpdate? proposed, CancellationToken cancellationToken) =>
        storageService.TestAsync(proposed, cancellationToken);
}
