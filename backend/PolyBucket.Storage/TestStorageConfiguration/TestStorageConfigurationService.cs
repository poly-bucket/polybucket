using System.Globalization;
using Microsoft.Extensions.Logging;
using PolyBucket.Database.Entities.SystemSettings;
using PolyBucket.Storage.UpdateStorageSettings;

namespace PolyBucket.Storage.TestStorageConfiguration;

public class TestStorageConfigurationService(
    IStorageSettingsResolver settingsResolver,
    IUpdateStorageSettingsRepository repository,
    TimeProvider timeProvider,
    ILogger<TestStorageConfigurationService> logger) : ITestStorageConfigurationService
{
    public async Task<TestStorageConfigurationResult> TestAsync(
        StorageSettingsUpdate? proposedUpdate,
        CancellationToken cancellationToken = default)
    {
        var current = await settingsResolver.GetEffectiveSettingsAsync(cancellationToken);
        var candidate = proposedUpdate == null
            ? current
            : MergeProposed(current, proposedUpdate);

        var errors = candidate.GetValidationErrors();
        if (errors.Count > 0)
        {
            return new TestStorageConfigurationResult
            {
                Success = false,
                Message = string.Join(" ", errors)
            };
        }

        try
        {
            await MinioStorageOperations
                .TestConnectivityAsync(candidate.ToStorageSettings(), cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LogWarning(
                ex,
                "Storage connectivity test failed for provider {Provider} at {Endpoint}:{Port}, bucket {BucketName}",
                candidate.Provider,
                candidate.Endpoint,
                candidate.Port,
                candidate.BucketName);

            return new TestStorageConfigurationResult
            {
                Success = false,
                Message =
                    $"Could not connect to {candidate.Provider} at {candidate.Endpoint}:{candidate.Port} (bucket: {candidate.BucketName}). Check the internal hostname, port, credentials, and that the service is running."
            };
        }

        var testedAt = timeProvider.GetUtcNow().UtcDateTime;
        if (proposedUpdate == null)
        {
            await repository.UpsertAsync(
                new Dictionary<string, string>
                {
                    [StorageSystemSettingKeys.LastSuccessfulTestAt] =
                        testedAt.ToString("O", CultureInfo.InvariantCulture)
                },
                cancellationToken);
            settingsResolver.Invalidate();
        }

        return new TestStorageConfigurationResult
        {
            Success = true,
            Message = "Object storage connection succeeded.",
            TestedAt = testedAt
        };
    }

    private static EffectiveStorageSettings MergeProposed(
        EffectiveStorageSettings current,
        StorageSettingsUpdate proposed)
    {
        var secret = proposed.ClearSecretKey
            ? string.Empty
            : !string.IsNullOrEmpty(proposed.SecretKey) ? proposed.SecretKey : current.SecretKey;

        return new EffectiveStorageSettings
        {
            Provider = string.IsNullOrWhiteSpace(proposed.Provider) ? current.Provider : proposed.Provider.Trim(),
            Endpoint = proposed.Endpoint.Trim(),
            Port = proposed.Port,
            UseSsl = proposed.UseSsl,
            BucketName = proposed.BucketName.Trim(),
            AccessKey = proposed.AccessKey.Trim(),
            SecretKey = secret,
            ExternalEndpoint = proposed.ExternalEndpoint?.Trim(),
            ExternalPort = proposed.ExternalPort,
            ExternalUseSsl = proposed.ExternalUseSsl,
            Region = string.IsNullOrWhiteSpace(proposed.Region) ? current.Region : proposed.Region.Trim(),
            ForcePathStyle = proposed.ForcePathStyle,
            IsConfigured = true,
            Sources = current.Sources
        };
    }
}
