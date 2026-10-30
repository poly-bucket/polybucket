using PolyBucket.Api.Features.Comments.LikeComment.Domain;

namespace PolyBucket.Api.Features.Comments.RemoveCommentLike.Repository;

public interface IRemoveCommentLikeRepository
{
    Task<CommentReactionOutcome> RemoveReactionAsync(Guid commentId, Guid userId, CancellationToken cancellationToken = default);
}
