using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using PolyBucket.Api.Features.Comments.LikeComment.Repository;
using PolyBucket.Api.Features.Comments.LikeComment.Domain;

namespace PolyBucket.Api.Features.Comments.LikeComment.Domain;

public interface ILikeCommentService
{
    Task<CommentReactionOutcome> ReactAsync(Guid commentId, Guid userId, CancellationToken cancellationToken = default);
}

public class LikeCommentService(ILikeCommentRepository repository) : ILikeCommentService
{
    public Task<CommentReactionOutcome> ReactAsync(Guid commentId, Guid userId, CancellationToken cancellationToken = default) =>
        repository.ReactAsync(commentId, userId, cancellationToken);
}
