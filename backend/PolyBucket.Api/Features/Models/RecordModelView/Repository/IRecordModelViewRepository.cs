using System;
using System.Threading;
using System.Threading.Tasks;
using PolyBucket.Api.Common.Models;

namespace PolyBucket.Api.Features.Models.RecordModelView.Repository;

public interface IRecordModelViewRepository
{
    Task<Model?> GetModelForViewAsync(Guid modelId, CancellationToken cancellationToken = default);

    Task<(int Views, bool Counted)> TryRecordViewAsync(
        Guid modelId,
        string viewerKey,
        DateTime nowUtc,
        DateTime dedupThresholdUtc,
        CancellationToken cancellationToken = default);
}
