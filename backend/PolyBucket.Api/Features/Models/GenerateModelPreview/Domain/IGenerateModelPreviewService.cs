using System;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Models.GenerateModelPreview.Domain;

public interface IGenerateModelPreviewService
{
    Task<GenerateModelPreviewResponse> RequestPreviewAsync(Guid modelId, string size, bool forceRegenerate, Guid userId, CancellationToken cancellationToken);
}
