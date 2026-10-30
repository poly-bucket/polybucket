using PolyBucket.Api.Features.Comments.LikeComment.Domain;
using PolyBucket.Database.Entities.Comments;

namespace PolyBucket.Api.Features.Comments.LikeComment.Repository;

public class LikeCommentRepository(ICommentReactionOperations operations) : ILikeCommentRepository
{
    public Task<CommentReactionOutcome> ReactAsync(Guid commentId, Guid userId, CancellationToken cancellationToken = default) =>
        operations.ReactAsync(commentId, userId, CommentReactionType.Like, cancellationToken);
}
