using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PolyBucket.Api.Features.Models.GenerateModelPreview.Repository;

namespace PolyBucket.Api.Features.Models.GenerateModelPreview.Domain;

public class ModelPreviewQueue(
    IModelPreviewQueueRepository repository,
    IModelPreviewSignal signal,
    IOptions<ModelPreviewOptions> options,
    TimeProvider timeProvider,
    ILogger<ModelPreviewQueue> logger) : IModelPreviewQueue
{
    public async Task<ModelPreview?> EnqueueAsync(Guid modelId, string size, bool forceRegenerate, CancellationToken cancellationToken)
    {
        if (await repository.GetModelAuthorIdAsync(modelId, cancellationToken) == null)
        {
            return null;
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var existing = await repository.GetAsync(modelId, size, cancellationToken);
        if (existing == null)
        {
            var preview = new ModelPreview
            {
                Id = Guid.NewGuid(),
                ModelId = modelId,
                Size = size,
                Status = PreviewStatus.Pending,
                CreatedAt = now,
                UpdatedAt = now
            };

            if (await repository.TryAddAsync(preview, cancellationToken))
            {
                signal.Notify();
                return preview;
            }

            existing = await repository.GetAsync(modelId, size, cancellationToken);
            if (existing == null)
            {
                return null;
            }
        }

        var shouldRequeue = existing.Status switch
        {
            PreviewStatus.Failed => true,
            PreviewStatus.Completed => forceRegenerate,
            _ => false
        };

        if (shouldRequeue)
        {
            existing.Status = PreviewStatus.Pending;
            existing.Attempts = 0;
            existing.NextAttemptAt = null;
            existing.LockedUntil = null;
            existing.ErrorMessage = null;
            existing.UpdatedAt = now;
            await repository.SaveChangesAsync(cancellationToken);
            signal.Notify();
        }

        return existing;
    }

    public async Task EnqueueForNewContentAsync(Guid modelId, CancellationToken cancellationToken)
    {
        try
        {
            if (!await repository.IsAutoGenerateEnabledAsync(cancellationToken))
            {
                return;
            }

            await EnqueueAsync(modelId, options.Value.DefaultSize, forceRegenerate: true, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(ex, "Could not queue a preview for model {ModelId}", modelId);
        }
    }
}
