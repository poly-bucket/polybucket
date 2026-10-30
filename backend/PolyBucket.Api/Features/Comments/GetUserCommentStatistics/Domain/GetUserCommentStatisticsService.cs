using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using PolyBucket.Api.Features.Comments.GetUserCommentStatistics.Repository;
using PolyBucket.Api.Features.Comments.Plugins;

namespace PolyBucket.Api.Features.Comments.GetUserCommentStatistics.Domain;

public interface IGetUserCommentStatisticsService
{
    Task<UserCommentStatistics> GetAsync(Guid userId, CancellationToken cancellationToken = default);
}

public class GetUserCommentStatisticsService(IGetUserCommentStatisticsRepository repository) : IGetUserCommentStatisticsService
{
    public Task<UserCommentStatistics> GetAsync(Guid userId, CancellationToken cancellationToken = default) =>
        repository.GetUserCommentStatisticsAsync(userId, cancellationToken);
}
