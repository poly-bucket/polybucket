using System;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Models.GenerateModelPreview.Domain;

public interface IModelPreviewQueue
{
    Task<ModelPreview?> EnqueueAsync(Guid modelId, string size, bool forceRegenerate, CancellationToken cancellationToken);

    Task EnqueueForNewContentAsync(Guid modelId, CancellationToken cancellationToken);
}
