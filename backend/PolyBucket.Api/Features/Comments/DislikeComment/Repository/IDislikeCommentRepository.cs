using PolyBucket.Api.Features.Comments.LikeComment.Domain;

namespace PolyBucket.Api.Features.Comments.DislikeComment.Repository;

public interface IDislikeCommentRepository
{
    Task<CommentReactionOutcome> ReactAsync(Guid commentId, Guid userId, CancellationToken cancellationToken = default);
}
