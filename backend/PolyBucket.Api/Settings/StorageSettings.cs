namespace PolyBucket.Api.Settings;

public class StorageSettings
{
    /// <summary>
    /// Provider name: MinIO, S3, AzureBlob (case-insensitive).
    /// </summary>
    public string Provider { get; set; } = "MinIO";

    /// <summary>
    /// Default bucket / container name to store all uploaded objects.
    /// </summary>
    public string BucketName { get; set; } = "polybucket-uploads";

    /// <summary>
    /// Endpoint or host (for MinIO / custom S3 implementations).
    /// Example: "localhost" or "s3.amazonaws.com".
    /// </summary>
    public string Endpoint { get; set; } = string.Empty;

    /// <summary>
    /// Endpoint port. Ignored for Azure Blob & AWS S3 default endpoints.
    /// </summary>
    public int Port { get; set; } = 0;

    /// <summary>
    /// Region for AWS S3-compatible providers.
    /// </summary>
    public string Region { get; set; } = "us-east-1";

    public bool UseSSL { get; set; } = false;

    public string AccessKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>
    /// For Azure Blob Storage we prefer a single connection string.
    /// </summary>
    public string? ConnectionString { get; set; }
} 