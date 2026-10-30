using System.Security.Cryptography;
using System.Text;
using PolyBucket.Storage.Settings;

namespace PolyBucket.Storage;

public enum StorageSettingSource
{
    Default,
    Environment,
    Database
}

public sealed class EffectiveStorageSettings
{
    public static class Fields
    {
        public const string Provider = nameof(Provider);
        public const string Endpoint = nameof(Endpoint);
        public const string Port = nameof(Port);
        public const string UseSsl = nameof(UseSsl);
        public const string BucketName = nameof(BucketName);
        public const string AccessKey = nameof(AccessKey);
        public const string SecretKey = nameof(SecretKey);
        public const string ExternalEndpoint = nameof(ExternalEndpoint);
        public const string ExternalPort = nameof(ExternalPort);
        public const string ExternalUseSsl = nameof(ExternalUseSsl);
        public const string Region = nameof(Region);
        public const string ForcePathStyle = nameof(ForcePathStyle);
    }

    public string Provider { get; init; } = StorageProviderIds.Minio;
    public string Endpoint { get; init; } = string.Empty;
    public int Port { get; init; }
    public bool UseSsl { get; init; }
    public string BucketName { get; init; } = string.Empty;
    public string AccessKey { get; init; } = string.Empty;
    public string SecretKey { get; init; } = string.Empty;
    public string? ExternalEndpoint { get; init; }
    public int? ExternalPort { get; init; }
    public bool? ExternalUseSsl { get; init; }
    public string Region { get; init; } = "us-east-1";
    public bool ForcePathStyle { get; init; }
    public DateTime? LastSuccessfulTestAt { get; init; }
    public bool IsConfigured { get; init; }
    public IReadOnlyDictionary<string, StorageSettingSource> Sources { get; init; } =
        new Dictionary<string, StorageSettingSource>();

    public string ConfigFingerprint
    {
        get
        {
            var raw =
                $"{Provider}|{Endpoint}|{Port}|{UseSsl}|{BucketName}|{AccessKey}|{SecretKey}|{ExternalEndpoint}|{ExternalPort}|{ExternalUseSsl}|{Region}|{ForcePathStyle}";
            return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(raw)));
        }
    }

    public bool IsManagedByEnvironment(string field) =>
        Sources.TryGetValue(field, out var source) && source == StorageSettingSource.Environment;

    public StorageSettings ToStorageSettings() => new()
    {
        Provider = Provider,
        Endpoint = Endpoint,
        Port = Port,
        UseSSL = UseSsl,
        BucketName = BucketName,
        AccessKey = AccessKey,
        SecretKey = SecretKey,
        ExternalEndpoint = ExternalEndpoint,
        ExternalPort = ExternalPort,
        ExternalUseSSL = ExternalUseSsl,
        Region = Region,
        ForcePathStyle = ForcePathStyle
    };

    public IReadOnlyList<string> GetValidationErrors()
    {
        if (!IsConfigured)
        {
            return [];
        }

        return StorageSettingsValidation.GetProfileValidationErrors(ToStorageSettings());
    }
}
