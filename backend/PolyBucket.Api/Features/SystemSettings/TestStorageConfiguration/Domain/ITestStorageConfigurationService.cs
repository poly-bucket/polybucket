using PolyBucket.Storage.TestStorageConfiguration;
using PolyBucket.Storage.UpdateStorageSettings;

namespace PolyBucket.Api.Features.SystemSettings.TestStorageConfiguration.Domain;

public interface ITestStorageConfigurationService
{
    Task<TestStorageConfigurationResult> TestStorageConfigurationAsync(StorageSettingsUpdate? proposed, CancellationToken cancellationToken = default);
}
