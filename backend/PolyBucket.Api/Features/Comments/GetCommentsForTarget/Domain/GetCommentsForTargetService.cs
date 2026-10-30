using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using PolyBucket.Api.Features.Comments.GetCommentsForTarget.Repository;
using PolyBucket.Api.Features.Comments.Plugins;
using PolyBucket.Database.Entities.Comments;

namespace PolyBucket.Api.Features.Comments.GetCommentsForTarget.Domain;

public interface IGetCommentsForTargetService
{
    Task<GetCommentsForTargetResult> GetAsync(string targetType, Guid targetId, int page, int pageSize, bool includeHidden, bool isModerator, Guid? currentUserId, bool isAdmin, CancellationToken cancellationToken = default);
}

public sealed record GetCommentsForTargetResult(CommentsPagedResponse? Page, string? ErrorMessage);

public class GetCommentsForTargetService(IGetCommentsForTargetRepository repository, ICommentResponseMapper mapper) : IGetCommentsForTargetService
{
    public async Task<GetCommentsForTargetResult> GetAsync(string targetType, Guid targetId, int page, int pageSize, bool includeHidden, bool isModerator, Guid? currentUserId, bool isAdmin, CancellationToken cancellationToken = default)
    {
        if (!CommentTargetParser.TryParse(targetType, targetId, out var target))
        {
            return new GetCommentsForTargetResult(null, $"Invalid target type: {targetType}");
        }

        if (page < 1 || pageSize < 1 || pageSize > CommentLimits.MaxPageSize)
        {
            return new GetCommentsForTargetResult(null, $"Page must be at least 1 and page size between 1 and {CommentLimits.MaxPageSize}");
        }

        var showHidden = includeHidden && isModerator;
        var comments = (await repository.GetCommentsForTargetAsync(target, showHidden, page, pageSize, cancellationToken)).ToList();
        var totalCount = await repository.GetCommentCountForTargetAsync(target, showHidden, cancellationToken);
        var statistics = await repository.GetCommentStatisticsAsync(target, cancellationToken);
        var mapped = await mapper.MapAsync(comments, currentUserId, isAdmin, includeReplies: true, cancellationToken);

        return new GetCommentsForTargetResult(new CommentsPagedResponse
        {
            Comments = mapped,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
            Statistics = statistics
        }, null);
    }
}
