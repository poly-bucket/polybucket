using PolyBucket.Api.Features.Comments.LikeComment.Domain;
using PolyBucket.Database.Entities.Comments;

namespace PolyBucket.Api.Features.Comments.DislikeComment.Repository;

public class DislikeCommentRepository(ICommentReactionOperations operations) : IDislikeCommentRepository
{
    public Task<CommentReactionOutcome> ReactAsync(Guid commentId, Guid userId, CancellationToken cancellationToken = default) =>
        operations.ReactAsync(commentId, userId, CommentReactionType.Dislike, cancellationToken);
}
