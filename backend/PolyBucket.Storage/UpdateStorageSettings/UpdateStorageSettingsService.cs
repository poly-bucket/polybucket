using System.Globalization;
using Microsoft.Extensions.Logging;
using PolyBucket.Database.Entities.SystemSettings;
using PolyBucket.Storage.Settings;
using Fields = PolyBucket.Storage.EffectiveStorageSettings.Fields;

namespace PolyBucket.Storage.UpdateStorageSettings;

public class UpdateStorageSettingsService(
    IUpdateStorageSettingsRepository repository,
    IStorageSettingsResolver settingsResolver,
    IStorageSecretProtector secretProtector,
    ILogger<UpdateStorageSettingsService> logger) : IUpdateStorageSettingsService
{
    public async Task<StorageSettingsDto> UpdateAsync(StorageSettingsUpdate update, CancellationToken cancellationToken = default)
    {
        var current = await settingsResolver.GetEffectiveSettingsAsync(cancellationToken);
        var normalized = Normalize(update);

        var conflicts = FindEnvironmentConflicts(current, normalized);
        if (conflicts.Count > 0)
        {
            throw new StorageSettingsConflictException(
                $"These settings are managed by environment variables and cannot be changed here: {string.Join(", ", conflicts)}.");
        }

        var secretAfterUpdate = normalized.ClearSecretKey
            ? string.Empty
            : !string.IsNullOrEmpty(normalized.SecretKey) ? normalized.SecretKey : current.SecretKey;

        var candidateSettings = new EffectiveStorageSettings
        {
            Provider = normalized.Provider,
            Endpoint = normalized.Endpoint,
            Port = normalized.Port,
            UseSsl = normalized.UseSsl,
            BucketName = normalized.BucketName,
            AccessKey = normalized.AccessKey,
            SecretKey = secretAfterUpdate,
            ExternalEndpoint = normalized.ExternalEndpoint,
            ExternalPort = normalized.ExternalPort,
            ExternalUseSsl = normalized.ExternalUseSsl,
            Region = normalized.Region,
            IsConfigured = true,
            Sources = current.Sources
        };

        var errors = candidateSettings.GetValidationErrors();
        if (errors.Count > 0)
        {
            throw new StorageSettingsValidationException(errors);
        }

        var values = BuildValues(normalized, secretAfterUpdate, current);
        await repository.UpsertAsync(values, cancellationToken);
        settingsResolver.Invalidate();

        logger.LogInformation("Storage settings updated for MinIO endpoint {Endpoint}:{Port}", normalized.Endpoint, normalized.Port);

        return StorageSettingsDto.From(await settingsResolver.GetEffectiveSettingsAsync(cancellationToken));
    }

    private static StorageSettingsUpdate Normalize(StorageSettingsUpdate update) =>
        update with
        {
            Provider = string.IsNullOrWhiteSpace(update.Provider) ? "minio" : update.Provider.Trim(),
            Endpoint = update.Endpoint?.Trim() ?? string.Empty,
            BucketName = update.BucketName?.Trim() ?? string.Empty,
            AccessKey = update.AccessKey?.Trim() ?? string.Empty,
            ExternalEndpoint = string.IsNullOrWhiteSpace(update.ExternalEndpoint) ? null : update.ExternalEndpoint.Trim(),
            Region = string.IsNullOrWhiteSpace(update.Region) ? "us-east-1" : update.Region.Trim()
        };

    private static List<string> FindEnvironmentConflicts(EffectiveStorageSettings current, StorageSettingsUpdate update)
    {
        var conflicts = new List<string>();

        void Check(string field, bool differs)
        {
            if (differs && current.IsManagedByEnvironment(field))
            {
                conflicts.Add(field);
            }
        }

        Check(Fields.Provider, !string.Equals(update.Provider, current.Provider, StringComparison.OrdinalIgnoreCase));
        Check(Fields.Endpoint, !string.Equals(update.Endpoint, current.Endpoint, StringComparison.Ordinal));
        Check(Fields.Port, update.Port != current.Port);
        Check(Fields.UseSsl, update.UseSsl != current.UseSsl);
        Check(Fields.BucketName, !string.Equals(update.BucketName, current.BucketName, StringComparison.Ordinal));
        Check(Fields.AccessKey, !string.Equals(update.AccessKey, current.AccessKey, StringComparison.Ordinal));
        Check(Fields.ExternalEndpoint, !string.Equals(update.ExternalEndpoint, current.ExternalEndpoint, StringComparison.Ordinal));
        Check(Fields.ExternalPort, update.ExternalPort != current.ExternalPort);
        Check(Fields.ExternalUseSsl, update.ExternalUseSsl != current.ExternalUseSsl);
        Check(Fields.Region, !string.Equals(update.Region, current.Region, StringComparison.OrdinalIgnoreCase));

        return conflicts;
    }

    private Dictionary<string, string> BuildValues(
        StorageSettingsUpdate update,
        string secretKey,
        EffectiveStorageSettings current)
    {
        var values = new Dictionary<string, string>
        {
            [StorageSystemSettingKeys.Provider] = update.Provider,
            [StorageSystemSettingKeys.Endpoint] = update.Endpoint,
            [StorageSystemSettingKeys.Port] = update.Port.ToString(CultureInfo.InvariantCulture),
            [StorageSystemSettingKeys.UseSsl] = update.UseSsl.ToString(),
            [StorageSystemSettingKeys.BucketName] = update.BucketName,
            [StorageSystemSettingKeys.AccessKey] = update.AccessKey,
            [StorageSystemSettingKeys.Region] = update.Region
        };

        if (update.ExternalEndpoint != null)
        {
            values[StorageSystemSettingKeys.ExternalEndpoint] = update.ExternalEndpoint;
        }

        if (update.ExternalPort.HasValue)
        {
            values[StorageSystemSettingKeys.ExternalPort] = update.ExternalPort.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (update.ExternalUseSsl.HasValue)
        {
            values[StorageSystemSettingKeys.ExternalUseSsl] = update.ExternalUseSsl.Value.ToString();
        }

        if (update.ClearSecretKey)
        {
            values[StorageSystemSettingKeys.SecretKey] = string.Empty;
        }
        else if (!string.IsNullOrEmpty(update.SecretKey))
        {
            values[StorageSystemSettingKeys.SecretKey] = secretProtector.Protect(update.SecretKey);
        }
        else if (!string.IsNullOrEmpty(current.SecretKey) && current.Sources.TryGetValue(Fields.SecretKey, out var source)
                 && source == StorageSettingSource.Database)
        {
            // keep existing encrypted value — do not re-write
        }
        else if (!string.IsNullOrEmpty(secretKey))
        {
            values[StorageSystemSettingKeys.SecretKey] = secretProtector.Protect(secretKey);
        }

        return values;
    }
}
