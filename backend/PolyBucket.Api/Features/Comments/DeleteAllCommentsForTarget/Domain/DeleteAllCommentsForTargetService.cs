using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using PolyBucket.Api.Features.Comments.DeleteAllCommentsForTarget.Repository;
using PolyBucket.Api.Features.Comments.GetCommentsForTarget.Domain;
using PolyBucket.Database.Entities.Comments;

namespace PolyBucket.Api.Features.Comments.DeleteAllCommentsForTarget.Domain;

public interface IDeleteAllCommentsForTargetService
{
    Task<DeleteAllCommentsForTargetResult> DeleteAllAsync(string targetType, Guid targetId, Guid userId, CancellationToken cancellationToken = default);
}

public sealed record DeleteAllCommentsForTargetResult(bool Success, string? ErrorMessage);

public class DeleteAllCommentsForTargetService(IDeleteAllCommentsForTargetRepository repository) : IDeleteAllCommentsForTargetService
{
    public async Task<DeleteAllCommentsForTargetResult> DeleteAllAsync(string targetType, Guid targetId, Guid userId, CancellationToken cancellationToken = default)
    {
        if (!CommentTargetParser.TryParse(targetType, targetId, out var target))
        {
            return new DeleteAllCommentsForTargetResult(false, $"Invalid target type: {targetType}");
        }

        var deleted = await repository.DeleteAllCommentsForTargetAsync(target, userId, cancellationToken);
        return deleted
            ? new DeleteAllCommentsForTargetResult(true, null)
            : new DeleteAllCommentsForTargetResult(false, "Unable to delete comments");
    }
}
