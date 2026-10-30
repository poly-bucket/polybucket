using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using PolyBucket.Api.Features.Comments.GetCommentStatistics.Repository;
using PolyBucket.Api.Features.Comments.GetCommentsForTarget.Domain;
using PolyBucket.Api.Features.Comments.Plugins;
using PolyBucket.Database.Entities.Comments;

namespace PolyBucket.Api.Features.Comments.GetCommentStatistics.Domain;

public interface IGetCommentStatisticsService
{
    Task<GetCommentStatisticsResult> GetAsync(string targetType, Guid targetId, CancellationToken cancellationToken = default);
}

public sealed record GetCommentStatisticsResult(CommentStatistics? Statistics, string? ErrorMessage);

public class GetCommentStatisticsService(IGetCommentStatisticsRepository repository) : IGetCommentStatisticsService
{
    public async Task<GetCommentStatisticsResult> GetAsync(string targetType, Guid targetId, CancellationToken cancellationToken = default)
    {
        if (!CommentTargetParser.TryParse(targetType, targetId, out var target))
        {
            return new GetCommentStatisticsResult(null, $"Invalid target type: {targetType}");
        }

        return new GetCommentStatisticsResult(await repository.GetCommentStatisticsAsync(target, cancellationToken), null);
    }
}
