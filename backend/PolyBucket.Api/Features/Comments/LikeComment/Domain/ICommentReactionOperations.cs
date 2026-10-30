using System;
using System.Threading;
using System.Threading.Tasks;
using PolyBucket.Database.Entities.Comments;

namespace PolyBucket.Api.Features.Comments.LikeComment.Domain;

public interface ICommentReactionOperations
{
    Task<CommentReactionOutcome> ReactAsync(Guid commentId, Guid userId, CommentReactionType type, CancellationToken cancellationToken = default);
    Task<CommentReactionOutcome> RemoveReactionAsync(Guid commentId, Guid userId, CommentReactionType type, CancellationToken cancellationToken = default);
}
