using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PolyBucket.Api.Settings;

namespace PolyBucket.Api.Common.Storage;

public sealed class StorageObjectKeyResolver(
    IOptions<StorageSettings> storageOptions,
    ILogger<StorageObjectKeyResolver> logger) : IStorageObjectKeyResolver
{
    private readonly StorageSettings _settings = storageOptions.Value;

    public string? Resolve(string? storedPathOrUrl)
    {
        if (string.IsNullOrEmpty(storedPathOrUrl))
        {
            return null;
        }

        if (!storedPathOrUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
        {
            return storedPathOrUrl;
        }

        try
        {
            var uri = new Uri(storedPathOrUrl);
            var pathSegments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
            var bucketIndex = Array.FindIndex(pathSegments, s => s == _settings.BucketName);
            if (bucketIndex >= 0 && bucketIndex + 1 < pathSegments.Length)
            {
                return string.Join("/", pathSegments.Skip(bucketIndex + 1));
            }

            logger.LogWarning(
                "Could not find bucket name '{BucketName}' in URL path: {StoredPathOrUrl}",
                _settings.BucketName,
                storedPathOrUrl);
            return null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to parse URL for object key extraction: {StoredPathOrUrl}", storedPathOrUrl);
            return null;
        }
    }
}
