using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Models.ModelReactions.Domain;

public interface IModelReactionService
{
    Task<ModelReactionOutcome> ReactAsync(Guid modelId, ClaimsPrincipal user, ModelReactionType type, CancellationToken cancellationToken = default);
    Task<ModelReactionOutcome> RemoveReactionAsync(Guid modelId, ClaimsPrincipal user, ModelReactionType type, CancellationToken cancellationToken = default);
}
