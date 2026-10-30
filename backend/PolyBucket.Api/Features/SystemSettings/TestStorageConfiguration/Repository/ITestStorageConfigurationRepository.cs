using PolyBucket.Storage.TestStorageConfiguration;
using PolyBucket.Storage.UpdateStorageSettings;

namespace PolyBucket.Api.Features.SystemSettings.TestStorageConfiguration.Repository;

public interface ITestStorageConfigurationRepository
{
    Task<TestStorageConfigurationResult> TestAsync(StorageSettingsUpdate? proposed, CancellationToken cancellationToken);
}
