using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PolyBucket.Database.Entities.Models;
using PolyBucket.Database.Persistence;
using PolyBucket.Storage;
using PolyBucket.Storage.Settings;

namespace PolyBucket.Api.Features.Files.StreamModelFile.Repository;

public class StreamModelFileRepository(
    PolyBucketDbContext context,
    IStorageService storageService,
    IOptions<StorageSettings> storageOptions) : IStreamModelFileRepository
{
    private readonly StorageSettings _storageSettings = storageOptions.Value;

    public Task<Model?> GetModelWithFilesAsync(Guid modelId, CancellationToken cancellationToken = default) =>
        context.Models
            .Include(m => m.Files)
            .Include(m => m.Author)
            .FirstOrDefaultAsync(m => m.Id == modelId, cancellationToken);

    public Task<Stream?> DownloadAsync(string objectKey, CancellationToken cancellationToken = default) =>
        storageService.DownloadAsync(objectKey);

    public async Task<(int Downloads, bool Counted)> TryRecordDownloadAsync(
        Guid modelId,
        string viewerKey,
        DateTime nowUtc,
        DateTime dedupThresholdUtc,
        CancellationToken cancellationToken = default)
    {
        var model = await context.Models
            .FirstOrDefaultAsync(m => m.Id == modelId && m.DeletedAt == null, cancellationToken);
        if (model == null)
        {
            return (0, false);
        }

        if (viewerKey.StartsWith("u:", StringComparison.Ordinal)
            && Guid.TryParse(viewerKey.AsSpan(2), out var userId)
            && model.AuthorId == userId)
        {
            return (model.Downloads, false);
        }

        var dedup = await context.ModelDownloadDedups
            .FirstOrDefaultAsync(d => d.ModelId == modelId && d.ViewerKey == viewerKey, cancellationToken);

        if (dedup != null && dedup.LastCountedAt > dedupThresholdUtc)
        {
            return (model.Downloads, false);
        }

        if (dedup == null)
        {
            context.ModelDownloadDedups.Add(new ModelDownloadDedup
            {
                Id = Guid.NewGuid(),
                ModelId = modelId,
                ViewerKey = viewerKey,
                LastCountedAt = nowUtc
            });
        }
        else
        {
            dedup.LastCountedAt = nowUtc;
        }

        model.Downloads++;
        await context.SaveChangesAsync(cancellationToken);
        return (model.Downloads, true);
    }

    public string ResolveObjectKey(string filePath)
    {
        if (!filePath.StartsWith("http", StringComparison.OrdinalIgnoreCase))
        {
            return filePath;
        }

        var uri = new Uri(filePath);
        var pathSegments = uri.AbsolutePath.Split('/');
        var bucketIndex = Array.IndexOf(pathSegments, _storageSettings.BucketName);
        if (bucketIndex >= 0 && bucketIndex + 1 < pathSegments.Length)
        {
            return string.Join("/", pathSegments.Skip(bucketIndex + 1));
        }

        return string.Empty;
    }
}
