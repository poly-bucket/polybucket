namespace PolyBucket.Storage;

public class StorageSettingsConflictException(string message) : Exception(message);

public class StorageSettingsValidationException(IReadOnlyList<string> errors) : Exception(string.Join(" ", errors))
{
    public IReadOnlyList<string> Errors { get; } = errors;
}
