using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using PolyBucket.Api.Features.Comments.Plugins;
using PolyBucket.Database.Entities.Comments;

namespace PolyBucket.Api.Features.Comments.GetCommentsForTarget.Repository;

public interface IGetCommentsForTargetRepository
{
    Task<IEnumerable<EnhancedComment>> GetCommentsForTargetAsync(CommentTarget target, bool includeHidden, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> GetCommentCountForTargetAsync(CommentTarget target, bool includeHidden, CancellationToken cancellationToken = default);
    Task<CommentStatistics> GetCommentStatisticsAsync(CommentTarget target, CancellationToken cancellationToken = default);
}

public class GetCommentsForTargetRepository(IEnhancedCommentsPlugin plugin) : IGetCommentsForTargetRepository
{
    public Task<IEnumerable<EnhancedComment>> GetCommentsForTargetAsync(CommentTarget target, bool includeHidden, int page, int pageSize, CancellationToken cancellationToken = default) =>
        plugin.GetCommentsForTargetAsync(target, includeHidden, page, pageSize);

    public Task<int> GetCommentCountForTargetAsync(CommentTarget target, bool includeHidden, CancellationToken cancellationToken = default) =>
        plugin.GetCommentCountForTargetAsync(target, includeHidden);

    public Task<CommentStatistics> GetCommentStatisticsAsync(CommentTarget target, CancellationToken cancellationToken = default) =>
        plugin.GetCommentStatisticsAsync(target);
}
