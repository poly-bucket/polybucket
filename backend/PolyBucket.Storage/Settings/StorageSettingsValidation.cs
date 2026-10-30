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

    public static bool IsCompleteS3CompatibleProfile(StorageSettings options)
    {
        if (!StorageProviderIds.TryNormalize(options.Provider, out _))
        {
            return false;
        }

        return !string.IsNullOrWhiteSpace(options.Endpoint)
               && options.Port is > 0 and <= 65535
               && !string.IsNullOrWhiteSpace(options.AccessKey)
               && !string.IsNullOrWhiteSpace(options.SecretKey)
               && !string.IsNullOrWhiteSpace(options.BucketName);
    }

    public static IReadOnlyList<string> GetProfileValidationErrors(StorageSettings options)
    {
        var errors = new List<string>();
        var provider = (options.Provider ?? string.Empty).Trim();

        if (!StorageProviderIds.TryNormalize(provider, out var normalized))
        {
            errors.Add(
                string.IsNullOrWhiteSpace(provider)
                    ? "Storage provider is required."
                    : $"Storage provider '{provider}' is not supported. Supported providers: minio, silo, seaweedfs.");
            return errors;
        }

        if (string.IsNullOrWhiteSpace(options.Endpoint))
        {
            errors.Add("Storage endpoint is required.");
        }

        if (options.Port is <= 0 or > 65535)
        {
            errors.Add("Storage port must be between 1 and 65535.");
        }

        if (string.IsNullOrWhiteSpace(options.AccessKey))
        {
            errors.Add("Storage access key is required.");
        }

        if (string.IsNullOrWhiteSpace(options.SecretKey))
        {
            errors.Add("Storage secret key is required.");
        }

        if (string.IsNullOrWhiteSpace(options.BucketName))
        {
            errors.Add("Storage bucket name is required.");
        }

        return errors;
    }
}
