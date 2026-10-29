namespace PolyBucket.Storage.UpdateStorageSettings;

public interface IUpdateStorageSettingsRepository
{
    Task UpsertAsync(IReadOnlyDictionary<string, string> values, CancellationToken cancellationToken = default);
}
