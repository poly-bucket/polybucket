using System;
using System.Threading;
using System.Threading.Tasks;
using PolyBucket.Api.Features.Models.RecordModelDownload.Repository;

namespace PolyBucket.Api.Features.Models.RecordModelDownload.Domain;

public class ModelDownloadCounter(
    IModelDownloadCounterRepository repository,
    TimeProvider timeProvider) : IModelDownloadCounter
{
    private static readonly TimeSpan DedupWindow = TimeSpan.FromHours(24);

    public async Task<(int Downloads, bool Counted)> TryRecordDownloadAsync(
        Guid modelId,
        string viewerKey,
        CancellationToken cancellationToken = default)
    {
        var model = await repository.GetModelAsync(modelId, cancellationToken);
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

        var nowUtc = timeProvider.GetUtcNow().UtcDateTime;
        var threshold = nowUtc - DedupWindow;
        return await repository.TryRecordDownloadAsync(
            modelId,
            viewerKey,
            nowUtc,
            threshold,
            cancellationToken);
    }
}
