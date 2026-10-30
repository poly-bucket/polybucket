using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using PolyBucket.Api.Features.Comments.RemoveCommentDislike.Repository;
using PolyBucket.Api.Features.Comments.LikeComment.Domain;

namespace PolyBucket.Api.Features.Comments.RemoveCommentDislike.Domain;

public interface IRemoveCommentDislikeService
{
    Task<CommentReactionOutcome> RemoveReactionAsync(Guid commentId, Guid userId, CancellationToken cancellationToken = default);
}

public class RemoveCommentDislikeService(IRemoveCommentDislikeRepository repository) : IRemoveCommentDislikeService
{
    public Task<CommentReactionOutcome> RemoveReactionAsync(Guid commentId, Guid userId, CancellationToken cancellationToken = default) =>
        repository.RemoveReactionAsync(commentId, userId, cancellationToken);
}
