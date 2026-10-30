using System.Globalization;
using Microsoft.Extensions.Logging;
using PolyBucket.Database.Entities.SystemSettings;

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
        var normalized = Normalize(update, current);

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
            ForcePathStyle = normalized.ForcePathStyle,
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

        logger.LogInformation(
            "Storage settings updated for provider {Provider} at {Endpoint}:{Port}",
            normalized.Provider,
            normalized.Endpoint,
            normalized.Port);

        return StorageSettingsDto.From(await settingsResolver.GetEffectiveSettingsAsync(cancellationToken));
    }

    private static StorageSettingsUpdate Normalize(StorageSettingsUpdate update, EffectiveStorageSettings current)
    {
        var provider = string.IsNullOrWhiteSpace(update.Provider) ? current.Provider : update.Provider.Trim();

        return update with
        {
            Provider = provider,
            Endpoint = update.Endpoint?.Trim() ?? string.Empty,
            BucketName = update.BucketName?.Trim() ?? string.Empty,
            AccessKey = update.AccessKey?.Trim() ?? string.Empty,
            ExternalEndpoint = string.IsNullOrWhiteSpace(update.ExternalEndpoint) ? null : update.ExternalEndpoint.Trim(),
            Region = string.IsNullOrWhiteSpace(update.Region) ? "us-east-1" : update.Region.Trim()
        };
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
            [StorageSystemSettingKeys.Region] = update.Region,
            [StorageSystemSettingKeys.ForcePathStyle] = update.ForcePathStyle.ToString()
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
        else if (!string.IsNullOrEmpty(current.SecretKey) && current.Sources.TryGetValue(EffectiveStorageSettings.Fields.SecretKey, out var source)
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
