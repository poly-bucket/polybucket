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
    }

    public string Provider { get; init; } = "minio";
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
    public DateTime? LastSuccessfulTestAt { get; init; }
    public bool IsConfigured { get; init; }
    public IReadOnlyDictionary<string, StorageSettingSource> Sources { get; init; } =
        new Dictionary<string, StorageSettingSource>();

    public string ConfigFingerprint
    {
        get
        {
            var raw =
                $"{Provider}|{Endpoint}|{Port}|{UseSsl}|{BucketName}|{AccessKey}|{SecretKey}|{ExternalEndpoint}|{ExternalPort}|{ExternalUseSsl}|{Region}";
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
        Region = Region
    };

    public IReadOnlyList<string> GetValidationErrors()
    {
        var errors = new List<string>();
        if (!IsConfigured)
        {
            return errors;
        }

        if (!Provider.Equals("minio", StringComparison.OrdinalIgnoreCase))
        {
            errors.Add($"Storage provider '{Provider}' is not supported. Only MinIO is currently supported.");
        }

        if (string.IsNullOrWhiteSpace(Endpoint))
        {
            errors.Add("Storage endpoint is required.");
        }

        if (Port is <= 0 or > 65535)
        {
            errors.Add("Storage port must be between 1 and 65535.");
        }

        if (string.IsNullOrWhiteSpace(AccessKey))
        {
            errors.Add("Storage access key is required.");
        }

        if (string.IsNullOrWhiteSpace(SecretKey))
        {
            errors.Add("Storage secret key is required.");
        }

        if (string.IsNullOrWhiteSpace(BucketName))
        {
            errors.Add("Storage bucket name is required.");
        }

        return errors;
    }
}
