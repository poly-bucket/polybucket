using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using PolyBucket.Api.Features.Comments.Plugins;

namespace PolyBucket.Api.Features.Comments.GetUserCommentStatistics.Repository;

public interface IGetUserCommentStatisticsRepository
{
    Task<UserCommentStatistics> GetUserCommentStatisticsAsync(Guid userId, CancellationToken cancellationToken = default);
}

public class GetUserCommentStatisticsRepository(IEnhancedCommentsPlugin plugin) : IGetUserCommentStatisticsRepository
{
    public Task<UserCommentStatistics> GetUserCommentStatisticsAsync(Guid userId, CancellationToken cancellationToken = default) =>
        plugin.GetUserCommentStatisticsAsync(userId);
}
