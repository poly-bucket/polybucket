using System;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Models.RecordModelDownload.Domain;

public interface IModelDownloadCounter
{
    Task<(int Downloads, bool Counted)> TryRecordDownloadAsync(
        Guid modelId,
        string viewerKey,
        CancellationToken cancellationToken = default);
}
