using PolyBucket.Api.Features.Comments.LikeComment.Domain;
using PolyBucket.Database.Entities.Comments;

namespace PolyBucket.Api.Features.Comments.RemoveCommentDislike.Repository;

public class RemoveCommentDislikeRepository(ICommentReactionOperations operations) : IRemoveCommentDislikeRepository
{
    public Task<CommentReactionOutcome> RemoveReactionAsync(Guid commentId, Guid userId, CancellationToken cancellationToken = default) =>
        operations.RemoveReactionAsync(commentId, userId, CommentReactionType.Dislike, cancellationToken);
}
