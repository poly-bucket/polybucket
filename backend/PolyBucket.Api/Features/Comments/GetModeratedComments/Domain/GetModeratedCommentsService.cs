using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using PolyBucket.Api.Features.Comments.GetCommentsForTarget.Domain;
using PolyBucket.Api.Features.Comments.GetModeratedComments.Repository;
using PolyBucket.Api.Features.Comments.Plugins;
using PolyBucket.Database.Entities.Comments;

namespace PolyBucket.Api.Features.Comments.GetModeratedComments.Domain;

public interface IGetModeratedCommentsService
{
    Task<GetModeratedCommentsResult> GetAsync(int page, int pageSize, Guid? currentUserId, bool isAdmin, CancellationToken cancellationToken = default);
}

public sealed record GetModeratedCommentsResult(IReadOnlyList<CommentResponse>? Comments, string? ErrorMessage);

public class GetModeratedCommentsService(IGetModeratedCommentsRepository repository, ICommentResponseMapper mapper) : IGetModeratedCommentsService
{
    public async Task<GetModeratedCommentsResult> GetAsync(int page, int pageSize, Guid? currentUserId, bool isAdmin, CancellationToken cancellationToken = default)
    {
        if (page < 1 || pageSize < 1 || pageSize > CommentLimits.MaxPageSize)
        {
            return new GetModeratedCommentsResult(null, $"Page must be at least 1 and page size between 1 and {CommentLimits.MaxPageSize}");
        }

        var comments = (await repository.GetModeratedCommentsAsync(page, pageSize, cancellationToken)).ToList();
        var mapped = await mapper.MapAsync(comments, currentUserId, isAdmin, includeReplies: false, cancellationToken);
        return new GetModeratedCommentsResult(mapped, null);
    }
}
