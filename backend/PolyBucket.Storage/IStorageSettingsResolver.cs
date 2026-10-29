namespace PolyBucket.Storage;

public interface IStorageSettingsResolver
{
    Task<EffectiveStorageSettings> GetEffectiveSettingsAsync(CancellationToken cancellationToken = default);
    void Invalidate();
}
