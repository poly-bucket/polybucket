using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using PolyBucket.Api.Features.Comments.RemoveCommentLike.Repository;
using PolyBucket.Api.Features.Comments.LikeComment.Domain;

namespace PolyBucket.Api.Features.Comments.RemoveCommentLike.Domain;

public interface IRemoveCommentLikeService
{
    Task<CommentReactionOutcome> RemoveReactionAsync(Guid commentId, Guid userId, CancellationToken cancellationToken = default);
}

public class RemoveCommentLikeService(IRemoveCommentLikeRepository repository) : IRemoveCommentLikeService
{
    public Task<CommentReactionOutcome> RemoveReactionAsync(Guid commentId, Guid userId, CancellationToken cancellationToken = default) =>
        repository.RemoveReactionAsync(commentId, userId, cancellationToken);
}
