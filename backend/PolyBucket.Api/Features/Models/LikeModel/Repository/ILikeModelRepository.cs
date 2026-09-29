using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Features.Models.LikeModel.Domain;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Models.LikeModel.Repository;

public interface ILikeModelRepository
{
    Task<Model?> GetModelByIdAsync(Guid modelId, CancellationToken cancellationToken);
    Task<Like?> FindLikeAsync(Guid modelId, Guid userId, CancellationToken cancellationToken);
    Task<bool> IsModelLikesEnabledAsync(CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
    void AddLike(Like like);
}
