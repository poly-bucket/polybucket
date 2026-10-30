using PolyBucket.Api.Features.Comments.LikeComment.Domain;

namespace PolyBucket.Api.Features.Comments.RemoveCommentDislike.Repository;

public interface IRemoveCommentDislikeRepository
{
    Task<CommentReactionOutcome> RemoveReactionAsync(Guid commentId, Guid userId, CancellationToken cancellationToken = default);
}
