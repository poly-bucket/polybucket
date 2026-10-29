using PolyBucket.Storage.UpdateStorageSettings;

namespace PolyBucket.Storage.TestStorageConfiguration;

public interface ITestStorageConfigurationService
{
    Task<TestStorageConfigurationResult> TestAsync(
        StorageSettingsUpdate? proposedUpdate,
        CancellationToken cancellationToken = default);
}

public sealed class TestStorageConfigurationResult
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public DateTime? TestedAt { get; init; }
}
