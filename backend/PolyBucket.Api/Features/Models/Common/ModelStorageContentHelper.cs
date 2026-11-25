using System.IO.Compression;
using PolyBucket.Storage;

namespace PolyBucket.Api.Features.Models.Common;

public static class ModelStorageContentHelper
{
    public const string CompressedSuffix = ".gz";

    public static string AppendCompressedSuffix(string objectKey) =>
        objectKey.EndsWith(CompressedSuffix, StringComparison.OrdinalIgnoreCase)
            ? objectKey
            : objectKey + CompressedSuffix;

    public static bool IsCompressedStoragePath(string? path) =>
        !string.IsNullOrEmpty(path) &&
        path.EndsWith(CompressedSuffix, StringComparison.OrdinalIgnoreCase);

    public static async Task<Stream> OpenStoredContentAsync(Stream raw, string storagePath, CancellationToken cancellationToken = default)
    {
        if (!IsCompressedStoragePath(storagePath))
        {
            return raw;
        }

        var decompressed = new MemoryStream();
        await using (raw)
        await using (var gzip = new GZipStream(raw, CompressionMode.Decompress, leaveOpen: false))
        {
            await gzip.CopyToAsync(decompressed, cancellationToken);
        }

        decompressed.Position = 0;
        return decompressed;
    }

    public static async Task<(string ObjectKey, long StoredSize)> CompressAndUploadAsync(
        IStorageService storage,
        string objectKey,
        Stream source,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        var compressedKey = AppendCompressedSuffix(objectKey);
        var compressed = new MemoryStream();
        await using (source)
        await using (var gzip = new GZipStream(compressed, CompressionLevel.Optimal, leaveOpen: true))
        {
            await source.CopyToAsync(gzip, cancellationToken);
        }

        compressed.Position = 0;
        await storage.UploadAsync(compressedKey, compressed, contentType, cancellationToken);
        return (compressedKey, compressed.Length);
    }
}