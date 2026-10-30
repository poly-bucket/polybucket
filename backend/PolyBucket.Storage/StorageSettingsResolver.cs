using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using PolyBucket.Database.Entities.SystemSettings;
using PolyBucket.Database.Persistence;

namespace PolyBucket.Storage;

public class StorageSettingsResolver(
    PolyBucketDbContext context,
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

        var settings = await BuildAsync(dbSettings, cancellationToken);
        cache.Set(CacheKey, settings, CacheDuration);
        return settings;
    }

    public void Invalidate()
    {
        cache.Remove(CacheKey);
    }

    private async Task<EffectiveStorageSettings> BuildAsync(
        IReadOnlyDictionary<string, string> db,
        CancellationToken cancellationToken)
    {
        var sources = new Dictionary<string, StorageSettingSource>();

        var provider = GetDbString(db, StorageSystemSettingKeys.Provider, StorageProviderIds.Minio, sources);
        var endpoint = GetDbString(db, StorageSystemSettingKeys.Endpoint, string.Empty, sources);
        var port = GetDbInt(db, StorageSystemSettingKeys.Port, 0, sources);
        var useSsl = GetDbBool(db, StorageSystemSettingKeys.UseSsl, false, sources);
        var bucket = GetDbString(db, StorageSystemSettingKeys.BucketName, string.Empty, sources);
        var accessKey = GetDbString(db, StorageSystemSettingKeys.AccessKey, string.Empty, sources);
        var secretKey = await ResolveSecretKeyAsync(db, sources, cancellationToken);
        var externalEndpoint = GetDbStringNullable(db, StorageSystemSettingKeys.ExternalEndpoint, sources);
        var externalPort = GetDbIntNullable(db, StorageSystemSettingKeys.ExternalPort, sources);
        var externalUseSsl = GetDbBoolNullable(db, StorageSystemSettingKeys.ExternalUseSsl, sources);
        var region = GetDbString(db, StorageSystemSettingKeys.Region, "us-east-1", sources);
        var forcePathStyle = ResolveForcePathStyle(db, provider, sources);

        DateTime? lastTest = DateTime.TryParse(
            GetRaw(db, StorageSystemSettingKeys.LastSuccessfulTestAt),
            CultureInfo.InvariantCulture,
            DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal,
            out var parsedLastTest)
            ? parsedLastTest
            : null;

        var candidate = new Settings.StorageSettings
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
            Region = region,
            ForcePathStyle = forcePathStyle
        };

        var dbComplete = HasDbStorageKeys(db)
            && StorageSettingsValidation.IsCompleteS3CompatibleProfile(candidate);

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
            ForcePathStyle = forcePathStyle,
            LastSuccessfulTestAt = lastTest,
            IsConfigured = dbComplete,
            Sources = sources
        };
    }

    private static bool HasDbStorageKeys(IReadOnlyDictionary<string, string> db) =>
        db.ContainsKey(StorageSystemSettingKeys.Endpoint)
        || db.ContainsKey(StorageSystemSettingKeys.BucketName);

    private static bool ResolveForcePathStyle(
        IReadOnlyDictionary<string, string> db,
        string provider,
        Dictionary<string, StorageSettingSource> sources)
    {
        var stored = GetRaw(db, StorageSystemSettingKeys.ForcePathStyle);
        if (bool.TryParse(stored, out var parsed))
        {
            sources[EffectiveStorageSettings.Fields.ForcePathStyle] = StorageSettingSource.Database;
            return parsed;
        }

        if (StorageProviderCatalog.TryGetDefaultForcePathStyle(provider, out var defaultValue))
        {
            sources[EffectiveStorageSettings.Fields.ForcePathStyle] = StorageSettingSource.Default;
            return defaultValue;
        }

        sources[EffectiveStorageSettings.Fields.ForcePathStyle] = StorageSettingSource.Default;
        return false;
    }

    private async Task<string> ResolveSecretKeyAsync(
        IReadOnlyDictionary<string, string> db,
        Dictionary<string, StorageSettingSource> sources,
        CancellationToken cancellationToken)
    {
        const string field = EffectiveStorageSettings.Fields.SecretKey;
        var stored = GetRaw(db, StorageSystemSettingKeys.SecretKey);
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

    private static string GetDbString(
        IReadOnlyDictionary<string, string> db,
        string key,
        string defaultValue,
        Dictionary<string, StorageSettingSource> sources)
    {
        var value = NullIfBlank(GetRaw(db, key));
        if (value != null)
        {
            sources[FieldForKey(key)] = StorageSettingSource.Database;
            return value;
        }

        sources[FieldForKey(key)] = StorageSettingSource.Default;
        return defaultValue;
    }

    private static string? GetDbStringNullable(
        IReadOnlyDictionary<string, string> db,
        string key,
        Dictionary<string, StorageSettingSource> sources)
    {
        var value = NullIfBlank(GetRaw(db, key));
        if (value != null)
        {
            sources[FieldForKey(key)] = StorageSettingSource.Database;
            return value;
        }

        sources[FieldForKey(key)] = StorageSettingSource.Default;
        return null;
    }

    private static int GetDbInt(
        IReadOnlyDictionary<string, string> db,
        string key,
        int defaultValue,
        Dictionary<string, StorageSettingSource> sources)
    {
        var parsed = ParseInt(GetRaw(db, key));
        if (parsed is > 0)
        {
            sources[FieldForKey(key)] = StorageSettingSource.Database;
            return parsed.Value;
        }

        sources[FieldForKey(key)] = StorageSettingSource.Default;
        return defaultValue;
    }

    private static int? GetDbIntNullable(
        IReadOnlyDictionary<string, string> db,
        string key,
        Dictionary<string, StorageSettingSource> sources)
    {
        var parsed = ParseInt(GetRaw(db, key));
        if (parsed.HasValue)
        {
            sources[FieldForKey(key)] = StorageSettingSource.Database;
            return parsed;
        }

        sources[FieldForKey(key)] = StorageSettingSource.Default;
        return null;
    }

    private static bool GetDbBool(
        IReadOnlyDictionary<string, string> db,
        string key,
        bool defaultValue,
        Dictionary<string, StorageSettingSource> sources)
    {
        var parsed = ParseBool(GetRaw(db, key));
        if (parsed.HasValue)
        {
            sources[FieldForKey(key)] = StorageSettingSource.Database;
            return parsed.Value;
        }

        sources[FieldForKey(key)] = StorageSettingSource.Default;
        return defaultValue;
    }

    private static bool? GetDbBoolNullable(
        IReadOnlyDictionary<string, string> db,
        string key,
        Dictionary<string, StorageSettingSource> sources)
    {
        var parsed = ParseBool(GetRaw(db, key));
        if (parsed.HasValue)
        {
            sources[FieldForKey(key)] = StorageSettingSource.Database;
            return parsed;
        }

        sources[FieldForKey(key)] = StorageSettingSource.Default;
        return null;
    }

    private static string FieldForKey(string storageKey) => storageKey switch
    {
        StorageSystemSettingKeys.Provider => EffectiveStorageSettings.Fields.Provider,
        StorageSystemSettingKeys.Endpoint => EffectiveStorageSettings.Fields.Endpoint,
        StorageSystemSettingKeys.Port => EffectiveStorageSettings.Fields.Port,
        StorageSystemSettingKeys.UseSsl => EffectiveStorageSettings.Fields.UseSsl,
        StorageSystemSettingKeys.BucketName => EffectiveStorageSettings.Fields.BucketName,
        StorageSystemSettingKeys.AccessKey => EffectiveStorageSettings.Fields.AccessKey,
        StorageSystemSettingKeys.SecretKey => EffectiveStorageSettings.Fields.SecretKey,
        StorageSystemSettingKeys.ExternalEndpoint => EffectiveStorageSettings.Fields.ExternalEndpoint,
        StorageSystemSettingKeys.ExternalPort => EffectiveStorageSettings.Fields.ExternalPort,
        StorageSystemSettingKeys.ExternalUseSsl => EffectiveStorageSettings.Fields.ExternalUseSsl,
        StorageSystemSettingKeys.Region => EffectiveStorageSettings.Fields.Region,
        StorageSystemSettingKeys.ForcePathStyle => EffectiveStorageSettings.Fields.ForcePathStyle,
        _ => storageKey
    };

    private static string? GetRaw(IReadOnlyDictionary<string, string> db, string key) =>
        db.TryGetValue(key, out var value) ? value : null;

    private static string? NullIfBlank(string? value) => string.IsNullOrWhiteSpace(value) ? null : value;

    private static int? ParseInt(string? value) => int.TryParse(value, out var parsed) ? parsed : null;

    private static bool? ParseBool(string? value) => bool.TryParse(value, out var parsed) ? parsed : null;
}
