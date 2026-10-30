using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using PolyBucket.Api.Features.Comments.Plugins;
using PolyBucket.Database.Entities.Comments;

namespace PolyBucket.Api.Features.Comments.UpdateComment.Repository;

public interface IUpdateCommentRepository
{
    Task<EnhancedComment> UpdateCommentAsync(Guid commentId, Guid userId, string content, CancellationToken cancellationToken = default);
}

public class UpdateCommentRepository(IEnhancedCommentsPlugin plugin) : IUpdateCommentRepository
{
    public Task<EnhancedComment> UpdateCommentAsync(Guid commentId, Guid userId, string content, CancellationToken cancellationToken = default) =>
        plugin.UpdateCommentAsync(commentId, userId, content);
}
