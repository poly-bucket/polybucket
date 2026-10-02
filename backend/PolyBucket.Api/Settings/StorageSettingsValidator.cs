using Microsoft.Extensions.Options;

namespace PolyBucket.Api.Settings;

public sealed class StorageSettingsValidator : IValidateOptions<StorageSettings>
{
    public ValidateOptionsResult Validate(string? name, StorageSettings options)
    {
        var provider = (options.Provider ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(provider))
        {
            return ValidateOptionsResult.Fail("Storage:Provider is required. Supported values: S3, SeaweedFS.");
        }

        if (!provider.Equals("s3", StringComparison.OrdinalIgnoreCase)
            && !provider.Equals("seaweedfs", StringComparison.OrdinalIgnoreCase))
        {
            return ValidateOptionsResult.Fail($"Storage:Provider '{provider}' is not supported. Supported values: S3, SeaweedFS.");
        }

        if (string.IsNullOrWhiteSpace((options.Endpoint ?? string.Empty).Trim()))
        {
            return ValidateOptionsResult.Fail("Storage:Endpoint (or environment variable Storage__Endpoint) is required for S3-compatible storage.");
        }

        if (options.Port <= 0 || options.Port > 65535)
        {
            return ValidateOptionsResult.Fail("Storage:Port must be set to a valid TCP port (1-65535) for S3-compatible storage.");
        }

        if (string.IsNullOrWhiteSpace((options.AccessKey ?? string.Empty).Trim()))
        {
            return ValidateOptionsResult.Fail("Storage:AccessKey (or environment variable Storage__AccessKey) is required.");
        }

        if (string.IsNullOrWhiteSpace((options.SecretKey ?? string.Empty).Trim()))
        {
            return ValidateOptionsResult.Fail("Storage:SecretKey (or environment variable Storage__SecretKey) is required.");
        }

        if (string.IsNullOrWhiteSpace((options.BucketName ?? string.Empty).Trim()))
        {
            return ValidateOptionsResult.Fail("Storage:BucketName (or environment variable Storage__BucketName) is required.");
        }

        return ValidateOptionsResult.Success;
    }
}
