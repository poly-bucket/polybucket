using System;
using System.Threading;
using System.Threading.Tasks;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Features.Models.ModelReactions.Domain;

namespace PolyBucket.Api.Features.Models.ModelReactions.Repository;

public interface IModelReactionRepository
{
    Task<Model?> GetModelByIdAsync(Guid modelId, CancellationToken cancellationToken = default);
    Task<bool> IsReactionsEnabledAsync(CancellationToken cancellationToken = default);
    Task<ModelReactionType?> GetUserReactionAsync(Guid modelId, Guid userId, CancellationToken cancellationToken = default);
    Task<(int Likes, int Dislikes)> GetCountsAsync(Guid modelId, CancellationToken cancellationToken = default);
    Task<bool> TryAddAsync(Guid modelId, Guid userId, ModelReactionType type, CancellationToken cancellationToken = default);
    Task<bool> TrySwitchAsync(Guid modelId, Guid userId, ModelReactionType from, ModelReactionType to, CancellationToken cancellationToken = default);
    Task<bool> TryRemoveAsync(Guid modelId, Guid userId, ModelReactionType type, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
