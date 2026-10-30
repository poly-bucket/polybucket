using PolyBucket.Api.Features.Comments.LikeComment.Domain;
using PolyBucket.Database.Entities.Comments;

namespace PolyBucket.Api.Features.Comments.RemoveCommentLike.Repository;

public class RemoveCommentLikeRepository(ICommentReactionOperations operations) : IRemoveCommentLikeRepository
{
    public Task<CommentReactionOutcome> RemoveReactionAsync(Guid commentId, Guid userId, CancellationToken cancellationToken = default) =>
        operations.RemoveReactionAsync(commentId, userId, CommentReactionType.Like, cancellationToken);
}
