using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using PolyBucket.Api.Features.Comments.Plugins;

namespace PolyBucket.Api.Features.Comments.DeleteComment.Repository;

public interface IDeleteCommentRepository
{
    Task<bool> DeleteCommentAsync(Guid commentId, Guid userId, bool isAdmin, CancellationToken cancellationToken = default);
}

public class DeleteCommentRepository(IEnhancedCommentsPlugin plugin) : IDeleteCommentRepository
{
    public Task<bool> DeleteCommentAsync(Guid commentId, Guid userId, bool isAdmin, CancellationToken cancellationToken = default) =>
        plugin.DeleteCommentAsync(commentId, userId, isAdmin);
}
