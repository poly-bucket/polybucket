using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.ACL.Services;
using PolyBucket.Api.Features.Models.GenerateModelPreview.Repository;

namespace PolyBucket.Api.Features.Models.GenerateModelPreview.Domain;

public class GenerateModelPreviewService(
    IModelPreviewQueueRepository repository,
    IModelPreviewQueue queue,
    IPermissionService permissionService,
    TimeProvider timeProvider) : IGenerateModelPreviewService
{
    public static readonly IReadOnlySet<string> SupportedSizes = new HashSet<string>(StringComparer.Ordinal) { "thumbnail", "medium", "large" };

    public async Task<GenerateModelPreviewResponse> RequestPreviewAsync(Guid modelId, string size, bool forceRegenerate, Guid userId, CancellationToken cancellationToken)
    {
        var normalizedSize = size.Trim().ToLowerInvariant();
        if (!SupportedSizes.Contains(normalizedSize))
        {
            throw new ArgumentException($"Size must be one of: {string.Join(", ", SupportedSizes)}", nameof(size));
        }

        var authorId = await repository.GetModelAuthorIdAsync(modelId, cancellationToken)
            ?? throw new KeyNotFoundException($"Model {modelId} not found");

        if (authorId != userId && !await permissionService.HasPermissionAsync(userId, PermissionConstants.MODEL_EDIT_ANY))
        {
            throw new UnauthorizedAccessException("Only the model's owner can request previews");
        }

        var preview = await queue.EnqueueAsync(modelId, normalizedSize, forceRegenerate, cancellationToken)
            ?? throw new KeyNotFoundException($"Model {modelId} not found");

        var isQueued = preview.Status is PreviewStatus.Pending or PreviewStatus.Generating;
        return new GenerateModelPreviewResponse
        {
            ModelId = modelId,
            Size = normalizedSize,
            Status = preview.Status,
            Message = isQueued ? "Preview generation queued" : "Preview already exists",
            IsQueued = isQueued,
            QueuedAt = timeProvider.GetUtcNow().UtcDateTime
        };
    }
}
