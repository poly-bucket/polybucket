using PolyBucket.Api.Common.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.ModelModeration.Domain;

public interface IModelModerationEnqueueService
{
    Task ApplyModerationStateForNewModelAsync(Model model, Guid authorId, CancellationToken cancellationToken = default);

    Task ApplyModerationStateForPublicVisibilityChangeAsync(Model model, Guid authorId, CancellationToken cancellationToken = default);
}
