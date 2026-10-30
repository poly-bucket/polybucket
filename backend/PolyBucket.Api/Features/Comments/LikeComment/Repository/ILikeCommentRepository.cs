using PolyBucket.Api.Features.Comments.LikeComment.Domain;

namespace PolyBucket.Api.Features.Comments.LikeComment.Repository;

public interface ILikeCommentRepository
{
    Task<CommentReactionOutcome> ReactAsync(Guid commentId, Guid userId, CancellationToken cancellationToken = default);
}
