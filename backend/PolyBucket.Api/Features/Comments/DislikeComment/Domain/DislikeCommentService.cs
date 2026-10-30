using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using PolyBucket.Api.Features.Comments.DislikeComment.Repository;
using PolyBucket.Api.Features.Comments.LikeComment.Domain;

namespace PolyBucket.Api.Features.Comments.DislikeComment.Domain;

public interface IDislikeCommentService
{
    Task<CommentReactionOutcome> ReactAsync(Guid commentId, Guid userId, CancellationToken cancellationToken = default);
}

public class DislikeCommentService(IDislikeCommentRepository repository) : IDislikeCommentService
{
    public Task<CommentReactionOutcome> ReactAsync(Guid commentId, Guid userId, CancellationToken cancellationToken = default) =>
        repository.ReactAsync(commentId, userId, cancellationToken);
}
