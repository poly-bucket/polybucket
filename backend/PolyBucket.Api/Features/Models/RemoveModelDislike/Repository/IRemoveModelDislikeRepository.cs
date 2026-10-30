using PolyBucket.Database.Entities.Models.Enums;
namespace PolyBucket.Api.Features.Models.RemoveModelDislike.Repository;
public interface IRemoveModelDislikeRepository
{
    Task<PolyBucket.Database.Entities.Models.Model?> GetModelByIdAsync(Guid modelId, CancellationToken cancellationToken = default);
    Task<ModelReactionType?> GetUserReactionAsync(Guid modelId, Guid userId, CancellationToken cancellationToken = default);
    Task<(int Likes, int Dislikes)> GetCountsAsync(Guid modelId, CancellationToken cancellationToken = default);
    Task<bool> TryRemoveAsync(Guid modelId, Guid userId, ModelReactionType type, CancellationToken cancellationToken = default);
}
