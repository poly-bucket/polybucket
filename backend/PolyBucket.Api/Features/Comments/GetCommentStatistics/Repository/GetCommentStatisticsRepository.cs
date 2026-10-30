using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using PolyBucket.Api.Features.Comments.Plugins;
using PolyBucket.Database.Entities.Comments;

namespace PolyBucket.Api.Features.Comments.GetCommentStatistics.Repository;

public interface IGetCommentStatisticsRepository
{
    Task<CommentStatistics> GetCommentStatisticsAsync(CommentTarget target, CancellationToken cancellationToken = default);
}

public class GetCommentStatisticsRepository(IEnhancedCommentsPlugin plugin) : IGetCommentStatisticsRepository
{
    public Task<CommentStatistics> GetCommentStatisticsAsync(CommentTarget target, CancellationToken cancellationToken = default) =>
        plugin.GetCommentStatisticsAsync(target);
}
