using System;
using System.Threading;
using System.Threading.Tasks;
using PolyBucket.Api.Common.Models;

namespace PolyBucket.Api.Features.Models.RecordModelDownload.Repository;

public interface IModelDownloadCounterRepository
{
    Task<Model?> GetModelAsync(Guid modelId, CancellationToken cancellationToken = default);

    Task<(int Downloads, bool Counted)> TryRecordDownloadAsync(
        Guid modelId,
        string viewerKey,
        DateTime nowUtc,
        DateTime dedupThresholdUtc,
        CancellationToken cancellationToken = default);
}
