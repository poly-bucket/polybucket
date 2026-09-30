using System;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Comments.Domain;

public interface ICommentReactionService
{
    Task<CommentReactionOutcome> ReactAsync(Guid commentId, Guid userId, CommentReactionType type, CancellationToken cancellationToken = default);
    Task<CommentReactionOutcome> RemoveReactionAsync(Guid commentId, Guid userId, CommentReactionType type, CancellationToken cancellationToken = default);
}
