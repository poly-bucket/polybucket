using PolyBucket.Database.Entities.Models;

namespace PolyBucket.Api.Features.Files.StreamModelFile.Repository;

public interface IStreamModelFileRepository
{
    Task<Model?> GetModelWithFilesAsync(Guid modelId, CancellationToken cancellationToken = default);
    Task<Stream?> DownloadAsync(string objectKey, CancellationToken cancellationToken = default);
    Task<(int Downloads, bool Counted)> TryRecordDownloadAsync(
        Guid modelId,
        string viewerKey,
        DateTime nowUtc,
        DateTime dedupThresholdUtc,
        CancellationToken cancellationToken = default);
    string ResolveObjectKey(string filePath);
}
