using PolyBucket.Storage.Settings;

namespace PolyBucket.Storage;

public static class StorageSettingsValidation
{
    public static bool IsEnvConfigurationAttempted(StorageSettings options)
    {
        return !string.IsNullOrWhiteSpace(options.Endpoint)
               || !string.IsNullOrWhiteSpace(options.AccessKey)
               || !string.IsNullOrWhiteSpace(options.SecretKey)
               || !string.IsNullOrWhiteSpace(options.BucketName);
    }

    public static bool IsCompleteMinioProfile(StorageSettings options)
    {
        return !string.IsNullOrWhiteSpace(options.Endpoint)
               && options.Port is > 0 and <= 65535
               && !string.IsNullOrWhiteSpace(options.AccessKey)
               && !string.IsNullOrWhiteSpace(options.SecretKey)
               && !string.IsNullOrWhiteSpace(options.BucketName)
               && (options.Provider ?? "minio").Equals("minio", StringComparison.OrdinalIgnoreCase);
    }
}
