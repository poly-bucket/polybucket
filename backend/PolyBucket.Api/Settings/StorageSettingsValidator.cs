using Microsoft.Extensions.Options;

namespace PolyBucket.Api.Settings;

public sealed class StorageSettingsValidator : IValidateOptions<StorageSettings>
{
    public ValidateOptionsResult Validate(string? name, StorageSettings options)
    {
        var provider = (options.Provider ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(provider))
        {
            return ValidateOptionsResult.Fail("Storage:Provider is required and currently only 'MinIO' is supported.");
        }

        if (!provider.Equals("minio", StringComparison.OrdinalIgnoreCase))
        {
            return ValidateOptionsResult.Fail($"Storage:Provider '{provider}' is not supported. Only 'MinIO' is currently supported.");
        }

        if (string.IsNullOrWhiteSpace((options.Endpoint ?? string.Empty).Trim()))
        {
            return ValidateOptionsResult.Fail("Storage:Endpoint (or environment variable Storage__Endpoint) is required for MinIO.");
        }

        if (options.Port <= 0 || options.Port > 65535)
        {
            return ValidateOptionsResult.Fail("Storage:Port must be set to a valid TCP port (1-65535) for MinIO.");
        }

        if (string.IsNullOrWhiteSpace((options.AccessKey ?? string.Empty).Trim()))
        {
            return ValidateOptionsResult.Fail("Storage:AccessKey (or environment variable Storage__AccessKey) is required for MinIO.");
        }

        if (string.IsNullOrWhiteSpace((options.SecretKey ?? string.Empty).Trim()))
        {
            return ValidateOptionsResult.Fail("Storage:SecretKey (or environment variable Storage__SecretKey) is required for MinIO.");
        }

        if (string.IsNullOrWhiteSpace((options.BucketName ?? string.Empty).Trim()))
        {
            return ValidateOptionsResult.Fail("Storage:BucketName (or environment variable Storage__BucketName) is required for MinIO.");
        }

        return ValidateOptionsResult.Success;
    }
}
