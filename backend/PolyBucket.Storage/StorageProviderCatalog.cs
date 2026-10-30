namespace PolyBucket.Storage;

public static class StorageProviderIds
{
    public const string Minio = "minio";
    public const string Silo = "silo";
    public const string SeaweedFs = "seaweedfs";

    public static readonly string[] All = [Minio, Silo, SeaweedFs];

    public static bool TryNormalize(string? value, out string normalized)
    {
        normalized = string.Empty;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        normalized = value.Trim().ToLowerInvariant();
        return IsSupported(normalized);
    }

    public static bool IsSupported(string? value) =>
        !string.IsNullOrWhiteSpace(value)
        && All.Contains(value.Trim().ToLowerInvariant(), StringComparer.Ordinal);
}

public sealed class StorageProviderDefinition
{
    public required string Id { get; init; }
    public required string DisplayName { get; init; }
    public required string Description { get; init; }
    public required StorageProviderDefaults Defaults { get; init; }
}

public sealed class StorageProviderDefaults
{
    public string Endpoint { get; init; } = string.Empty;
    public int Port { get; init; } = 9000;
    public bool UseSsl { get; init; }
    public string BucketName { get; init; } = "polybucket-uploads";
    public string ExternalEndpoint { get; init; } = "localhost";
    public int ExternalPort { get; init; } = 9000;
    public bool ExternalUseSsl { get; init; }
    public string Region { get; init; } = "us-east-1";
    public bool ForcePathStyle { get; init; }
}

public static class StorageProviderCatalog
{
    private static readonly IReadOnlyList<StorageProviderDefinition> Providers =
    [
        new StorageProviderDefinition
        {
            Id = StorageProviderIds.Minio,
            DisplayName = "MinIO",
            Description =
                "MinIO or another S3-compatible server. Use the Docker service hostname for internal access and a public hostname for browser downloads.",
            Defaults = new StorageProviderDefaults
            {
                Endpoint = string.Empty,
                Port = 9000,
                ForcePathStyle = false
            }
        },
        new StorageProviderDefinition
        {
            Id = StorageProviderIds.Silo,
            DisplayName = "Silo",
            Description =
                "Silo (S3-compatible). In Docker Compose, the bundled service is named silo on port 9000.",
            Defaults = new StorageProviderDefaults
            {
                Endpoint = "silo",
                Port = 9000,
                ForcePathStyle = false
            }
        },
        new StorageProviderDefinition
        {
            Id = StorageProviderIds.SeaweedFs,
            DisplayName = "SeaweedFS",
            Description =
                "SeaweedFS S3 gateway. Default gateway port is 8333; path-style access is often required.",
            Defaults = new StorageProviderDefaults
            {
                Endpoint = string.Empty,
                Port = 8333,
                ForcePathStyle = true
            }
        }
    ];

    public static IReadOnlyList<StorageProviderDefinition> GetAll() => Providers;

    public static StorageProviderDefinition? TryGet(string? providerId)
    {
        if (!StorageProviderIds.TryNormalize(providerId, out var id))
        {
            return null;
        }

        return Providers.FirstOrDefault(p => p.Id == id);
    }

    public static bool TryGetDefaultForcePathStyle(string? providerId, out bool forcePathStyle)
    {
        forcePathStyle = false;
        var definition = TryGet(providerId);
        if (definition == null)
        {
            return false;
        }

        forcePathStyle = definition.Defaults.ForcePathStyle;
        return true;
    }
}
