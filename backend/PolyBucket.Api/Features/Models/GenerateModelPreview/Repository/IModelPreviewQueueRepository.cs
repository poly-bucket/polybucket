using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PolyBucket.Api.Features.Models.GenerateModelPreview.Domain;

namespace PolyBucket.Api.Features.Models.GenerateModelPreview.Repository;

public interface IModelPreviewQueueRepository
{
    Task<ModelPreview?> GetAsync(Guid modelId, string size, CancellationToken cancellationToken);
    Task<Guid?> GetModelAuthorIdAsync(Guid modelId, CancellationToken cancellationToken);
    Task<bool> IsAutoGenerateEnabledAsync(CancellationToken cancellationToken);
    Task<bool> TryAddAsync(ModelPreview preview, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<ModelPreview>> ClaimBatchAsync(int batchSize, TimeSpan lockDuration, DateTime now, CancellationToken cancellationToken);
    Task<IReadOnlyList<ModelPreviewSourceFile>> GetCandidateFilesAsync(Guid modelId, CancellationToken cancellationToken);
    Task MarkCompletedAsync(Guid previewId, ModelPreview result, DateTime now, CancellationToken cancellationToken);
    Task MarkRetryAsync(Guid previewId, string error, DateTime nextAttemptAt, CancellationToken cancellationToken);
    Task MarkFailedAsync(Guid previewId, string error, DateTime now, CancellationToken cancellationToken);
}
