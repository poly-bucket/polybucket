namespace PolyBucket.Storage.UpdateStorageSettings;

public sealed record StorageSettingsUpdate
{
    public string Provider { get; init; } = "minio";
    public string Endpoint { get; init; } = string.Empty;
    public int Port { get; init; } = 9000;
    public bool UseSsl { get; init; }
    public string BucketName { get; init; } = string.Empty;
    public string AccessKey { get; init; } = string.Empty;
    public string? SecretKey { get; init; }
    public bool ClearSecretKey { get; init; }
    public string? ExternalEndpoint { get; init; }
    public int? ExternalPort { get; init; }
    public bool? ExternalUseSsl { get; init; }
    public string Region { get; init; } = "us-east-1";
}
