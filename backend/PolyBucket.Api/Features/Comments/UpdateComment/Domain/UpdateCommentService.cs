using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using PolyBucket.Api.Features.Comments.GetCommentsForTarget.Domain;
using PolyBucket.Api.Features.Comments.Plugins;
using PolyBucket.Api.Features.Comments.UpdateComment.Repository;

namespace PolyBucket.Api.Features.Comments.UpdateComment.Domain;

public interface IUpdateCommentService
{
    Task<UpdateCommentResult> UpdateAsync(Guid commentId, Guid userId, string content, bool isAdmin, CancellationToken cancellationToken = default);
}

public sealed record UpdateCommentResult(CommentResponse? Comment, string? ErrorMessage, bool Forbidden);

public class UpdateCommentService(IUpdateCommentRepository repository, ICommentResponseMapper mapper) : IUpdateCommentService
{
    public async Task<UpdateCommentResult> UpdateAsync(Guid commentId, Guid userId, string content, bool isAdmin, CancellationToken cancellationToken = default)
    {
        try
        {
            var comment = await repository.UpdateCommentAsync(commentId, userId, content, cancellationToken);
            var mapped = await mapper.MapAsync(new[] { comment }, userId, isAdmin, includeReplies: true, cancellationToken);
            return new UpdateCommentResult(mapped[0], null, false);
        }
        catch (InvalidOperationException ex)
        {
            return new UpdateCommentResult(null, ex.Message, false);
        }
        catch (UnauthorizedAccessException)
        {
            return new UpdateCommentResult(null, null, true);
        }
    }
}
