using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PolyBucket.Database.Entities.SystemSettings;
using PolyBucket.Database.Persistence;
using PolyBucket.Storage.Settings;

namespace PolyBucket.Storage;

public class StorageSettingsResolver(
    PolyBucketDbContext context,
    IOptionsMonitor<StorageSettings> envOptions,
    IConfiguration configuration,
    IStorageSecretProtector secretProtector,
    IMemoryCache cache,
    ILogger<StorageSettingsResolver> logger) : IStorageSettingsResolver
{
    public const string CacheKey = "storage:effective-settings";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromSeconds(30);

    public async Task<EffectiveStorageSettings> GetEffectiveSettingsAsync(CancellationToken cancellationToken = default)
    {
        if (cache.TryGetValue(CacheKey, out EffectiveStorageSettings? cached) && cached != null)
        {
            return cached;
        }

        var dbSettings = await context.SystemSettings
            .Where(s => s.Key.StartsWith("Storage:"))
            .ToDictionaryAsync(s => s.Key, s => s.Value, cancellationToken);

        var settings = await BuildAsync(envOptions.CurrentValue, dbSettings, cancellationToken);
        cache.Set(CacheKey, settings, CacheDuration);
        return settings;
    }

    public void Invalidate()
    {
        cache.Remove(CacheKey);
    }

    private async Task<EffectiveStorageSettings> BuildAsync(
        StorageSettings env,
        IReadOnlyDictionary<string, string> db,
        CancellationToken cancellationToken)
    {
        var sources = new Dictionary<string, StorageSettingSource>();

        var provider = PickString(
            EffectiveStorageSettings.Fields.Provider,
            NullIfBlank(env.Provider),
            Get(db, StorageSystemSettingKeys.Provider),
            "minio",
            sources);

        var endpoint = PickString(
            EffectiveStorageSettings.Fields.Endpoint,
            NullIfBlank(env.Endpoint),
            Get(db, StorageSystemSettingKeys.Endpoint),
            string.Empty,
            sources);

        var port = PickInt(
            EffectiveStorageSettings.Fields.Port,
            env.Port > 0 ? env.Port : null,
            ParseInt(Get(db, StorageSystemSettingKeys.Port)),
            0,
            sources);

        var useSsl = ResolveBool(
            EffectiveStorageSettings.Fields.UseSsl,
            configuration.GetSection("Storage").GetValue<bool?>("UseSSL")
                ?? configuration.GetSection("AppSettings:Storage").GetValue<bool?>("UseSSL"),
            ParseBool(Get(db, StorageSystemSettingKeys.UseSsl)),
            false,
            sources);

        var bucket = PickString(
            EffectiveStorageSettings.Fields.BucketName,
            NullIfBlank(env.BucketName),
            Get(db, StorageSystemSettingKeys.BucketName),
            string.Empty,
            sources);

        var accessKey = PickString(
            EffectiveStorageSettings.Fields.AccessKey,
            NullIfBlank(env.AccessKey),
            Get(db, StorageSystemSettingKeys.AccessKey),
            string.Empty,
            sources);

        var secretKey = await ResolveSecretKeyAsync(env, db, sources, cancellationToken);

        var externalEndpoint = PickStringNullable(
            EffectiveStorageSettings.Fields.ExternalEndpoint,
            NullIfBlank(env.ExternalEndpoint),
            Get(db, StorageSystemSettingKeys.ExternalEndpoint),
            sources);

        var externalPort = PickIntNullable(
            EffectiveStorageSettings.Fields.ExternalPort,
            env.ExternalPort,
            ParseInt(Get(db, StorageSystemSettingKeys.ExternalPort)),
            sources);

        var externalUseSsl = ResolveBoolNullable(
            EffectiveStorageSettings.Fields.ExternalUseSsl,
            configuration.GetSection("Storage").GetValue<bool?>("ExternalUseSSL")
                ?? configuration.GetSection("AppSettings:Storage").GetValue<bool?>("ExternalUseSSL"),
            ParseBool(Get(db, StorageSystemSettingKeys.ExternalUseSsl)),
            sources);

        var region = PickString(
            EffectiveStorageSettings.Fields.Region,
            NullIfBlank(env.Region),
            Get(db, StorageSystemSettingKeys.Region),
            "us-east-1",
            sources);

        DateTime? lastTest = DateTime.TryParse(
            Get(db, StorageSystemSettingKeys.LastSuccessfulTestAt),
            CultureInfo.InvariantCulture,
            DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal,
            out var parsedLastTest)
            ? parsedLastTest
            : null;

        var candidate = new StorageSettings
        {
            Provider = provider,
            Endpoint = endpoint,
            Port = port,
            UseSSL = useSsl,
            BucketName = bucket,
            AccessKey = accessKey,
            SecretKey = secretKey,
            ExternalEndpoint = externalEndpoint,
            ExternalPort = externalPort,
            ExternalUseSSL = externalUseSsl,
            Region = region
        };

        var envComplete = StorageSettingsValidation.IsEnvConfigurationAttempted(env)
            && StorageSettingsValidation.IsCompleteMinioProfile(env);
        var dbComplete = HasDbStorageKeys(db) && StorageSettingsValidation.IsCompleteMinioProfile(candidate);
        var isConfigured = envComplete || dbComplete;

        return new EffectiveStorageSettings
        {
            Provider = provider,
            Endpoint = endpoint,
            Port = port,
            UseSsl = useSsl,
            BucketName = bucket,
            AccessKey = accessKey,
            SecretKey = secretKey,
            ExternalEndpoint = externalEndpoint,
            ExternalPort = externalPort,
            ExternalUseSsl = externalUseSsl,
            Region = region,
            LastSuccessfulTestAt = lastTest,
            IsConfigured = isConfigured,
            Sources = sources
        };
    }

    private static bool HasDbStorageKeys(IReadOnlyDictionary<string, string> db) =>
        db.ContainsKey(StorageSystemSettingKeys.Endpoint)
        || db.ContainsKey(StorageSystemSettingKeys.BucketName);

    private async Task<string> ResolveSecretKeyAsync(
        StorageSettings env,
        IReadOnlyDictionary<string, string> db,
        Dictionary<string, StorageSettingSource> sources,
        CancellationToken cancellationToken)
    {
        const string field = EffectiveStorageSettings.Fields.SecretKey;
        if (!string.IsNullOrWhiteSpace(env.SecretKey))
        {
            sources[field] = StorageSettingSource.Environment;
            return env.SecretKey;
        }

        var stored = Get(db, StorageSystemSettingKeys.SecretKey);
        if (string.IsNullOrEmpty(stored))
        {
            sources[field] = StorageSettingSource.Default;
            return string.Empty;
        }

        sources[field] = StorageSettingSource.Database;
        var result = secretProtector.Unprotect(stored);
        if (result.IsUnreadable)
        {
            logger.LogError(
                "The stored storage secret key could not be decrypted. Data Protection keys may have changed; re-enter the secret in storage settings.");
            return string.Empty;
        }

        if (result.IsLegacyPlaintext)
        {
            await UpgradeLegacySecretAsync(result.Plaintext, cancellationToken);
        }

        return result.Plaintext;
    }

    private async Task UpgradeLegacySecretAsync(string plaintext, CancellationToken cancellationToken)
    {
        try
        {
            var setting = await context.SystemSettings
                .FirstOrDefaultAsync(s => s.Key == StorageSystemSettingKeys.SecretKey, cancellationToken);
            if (setting == null)
            {
                return;
            }

            setting.Value = secretProtector.Protect(plaintext);
            await context.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Upgraded stored storage secret key to encrypted storage");
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to upgrade legacy storage secret key to encrypted storage");
        }
    }

    private static string PickString(
        string field,
        string? environmentValue,
        string? databaseValue,
        string defaultValue,
        Dictionary<string, StorageSettingSource> sources)
    {
        if (!string.IsNullOrWhiteSpace(environmentValue))
        {
            sources[field] = StorageSettingSource.Environment;
            return environmentValue;
        }

        if (!string.IsNullOrWhiteSpace(databaseValue))
        {
            sources[field] = StorageSettingSource.Database;
            return databaseValue;
        }

        sources[field] = StorageSettingSource.Default;
        return defaultValue;
    }

    private static string? PickStringNullable(
        string field,
        string? environmentValue,
        string? databaseValue,
        Dictionary<string, StorageSettingSource> sources)
    {
        if (!string.IsNullOrWhiteSpace(environmentValue))
        {
            sources[field] = StorageSettingSource.Environment;
            return environmentValue;
        }

        if (!string.IsNullOrWhiteSpace(databaseValue))
        {
            sources[field] = StorageSettingSource.Database;
            return databaseValue;
        }

        sources[field] = StorageSettingSource.Default;
        return null;
    }

    private static int PickInt(
        string field,
        int? environmentValue,
        int? databaseValue,
        int defaultValue,
        Dictionary<string, StorageSettingSource> sources)
    {
        if (environmentValue is > 0)
        {
            sources[field] = StorageSettingSource.Environment;
            return environmentValue.Value;
        }

        if (databaseValue is > 0)
        {
            sources[field] = StorageSettingSource.Database;
            return databaseValue.Value;
        }

        sources[field] = StorageSettingSource.Default;
        return defaultValue;
    }

    private static int? PickIntNullable(
        string field,
        int? environmentValue,
        int? databaseValue,
        Dictionary<string, StorageSettingSource> sources)
    {
        if (environmentValue.HasValue)
        {
            sources[field] = StorageSettingSource.Environment;
            return environmentValue;
        }

        if (databaseValue.HasValue)
        {
            sources[field] = StorageSettingSource.Database;
            return databaseValue;
        }

        sources[field] = StorageSettingSource.Default;
        return null;
    }

    private static bool ResolveBool(
        string field,
        bool? environmentValue,
        bool? databaseValue,
        bool defaultValue,
        Dictionary<string, StorageSettingSource> sources)
    {
        if (environmentValue.HasValue)
        {
            sources[field] = StorageSettingSource.Environment;
            return environmentValue.Value;
        }

        if (databaseValue.HasValue)
        {
            sources[field] = StorageSettingSource.Database;
            return databaseValue.Value;
        }

        sources[field] = StorageSettingSource.Default;
        return defaultValue;
    }

    private static bool? ResolveBoolNullable(
        string field,
        bool? environmentValue,
        bool? databaseValue,
        Dictionary<string, StorageSettingSource> sources)
    {
        if (environmentValue.HasValue)
        {
            sources[field] = StorageSettingSource.Environment;
            return environmentValue;
        }

        if (databaseValue.HasValue)
        {
            sources[field] = StorageSettingSource.Database;
            return databaseValue;
        }

        sources[field] = StorageSettingSource.Default;
        return null;
    }

    private static string? Get(IReadOnlyDictionary<string, string> db, string key) =>
        db.TryGetValue(key, out var value) ? NullIfBlank(value) : null;

    private static string? NullIfBlank(string? value) => string.IsNullOrWhiteSpace(value) ? null : value;

    private static int? ParseInt(string? value) => int.TryParse(value, out var parsed) ? parsed : null;

    private static bool? ParseBool(string? value) => bool.TryParse(value, out var parsed) ? parsed : null;
}
