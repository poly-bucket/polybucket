using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using PolyBucket.Api.Features.Comments.Plugins;
using PolyBucket.Database.Entities.Comments;

namespace PolyBucket.Api.Features.Comments.DeleteAllCommentsForTarget.Repository;

public interface IDeleteAllCommentsForTargetRepository
{
    Task<bool> DeleteAllCommentsForTargetAsync(CommentTarget target, Guid deletedByUserId, CancellationToken cancellationToken = default);
}

public class DeleteAllCommentsForTargetRepository(IEnhancedCommentsPlugin plugin) : IDeleteAllCommentsForTargetRepository
{
    public Task<bool> DeleteAllCommentsForTargetAsync(CommentTarget target, Guid deletedByUserId, CancellationToken cancellationToken = default) =>
        plugin.DeleteAllCommentsForTargetAsync(target, deletedByUserId);
}
