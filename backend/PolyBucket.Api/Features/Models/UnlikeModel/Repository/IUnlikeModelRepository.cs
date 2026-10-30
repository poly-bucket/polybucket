using PolyBucket.Database.Entities.Models.Enums;
using PolyBucket.Api.Features.Models.LikeModel.Domain;

namespace PolyBucket.Api.Features.Models.UnlikeModel.Repository;

public interface IUnlikeModelRepository
{
    Task<PolyBucket.Database.Entities.Models.Model?> GetModelByIdAsync(Guid modelId, CancellationToken cancellationToken = default);
    Task<ModelReactionType?> GetUserReactionAsync(Guid modelId, Guid userId, CancellationToken cancellationToken = default);
    Task<(int Likes, int Dislikes)> GetCountsAsync(Guid modelId, CancellationToken cancellationToken = default);
    Task<bool> TryRemoveAsync(Guid modelId, Guid userId, ModelReactionType type, CancellationToken cancellationToken = default);
}
