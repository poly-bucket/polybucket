using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using PolyBucket.Api.Features.Comments.GetComment.Repository;
using PolyBucket.Api.Features.Comments.GetCommentsForTarget.Domain;
using PolyBucket.Api.Features.Comments.Plugins;
using PolyBucket.Database.Entities.Comments;

namespace PolyBucket.Api.Features.Comments.GetComment.Domain;

public interface IGetCommentService
{
    Task<GetCommentResult> GetAsync(Guid commentId, Guid? currentUserId, bool isAdmin, bool isModerator, CancellationToken cancellationToken = default);
}

public sealed record GetCommentResult(CommentResponse? Comment, bool NotFound);

public class GetCommentService(IGetCommentRepository repository, ICommentResponseMapper mapper) : IGetCommentService
{
    public async Task<GetCommentResult> GetAsync(Guid commentId, Guid? currentUserId, bool isAdmin, bool isModerator, CancellationToken cancellationToken = default)
    {
        var comment = await repository.GetCommentByIdAsync(commentId, cancellationToken);
        if (comment == null || (comment.IsHidden && !isModerator))
        {
            return new GetCommentResult(null, true);
        }

        var mapped = await mapper.MapAsync(new[] { comment }, currentUserId, isAdmin, includeReplies: true, cancellationToken);
        return new GetCommentResult(mapped[0], false);
    }
}
