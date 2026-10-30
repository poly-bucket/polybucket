using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using PolyBucket.Api.Features.Comments.Plugins;
using PolyBucket.Database.Entities.Comments;

namespace PolyBucket.Api.Features.Comments.GetModeratedComments.Repository;

public interface IGetModeratedCommentsRepository
{
    Task<IEnumerable<EnhancedComment>> GetModeratedCommentsAsync(int page, int pageSize, CancellationToken cancellationToken = default);
}

public class GetModeratedCommentsRepository(IEnhancedCommentsPlugin plugin) : IGetModeratedCommentsRepository
{
    public Task<IEnumerable<EnhancedComment>> GetModeratedCommentsAsync(int page, int pageSize, CancellationToken cancellationToken = default) =>
        plugin.GetModeratedCommentsAsync(page, pageSize);
}
