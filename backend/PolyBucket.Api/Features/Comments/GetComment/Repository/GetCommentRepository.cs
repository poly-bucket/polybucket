using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using PolyBucket.Api.Features.Comments.Plugins;
using PolyBucket.Database.Entities.Comments;

namespace PolyBucket.Api.Features.Comments.GetComment.Repository;

public interface IGetCommentRepository
{
    Task<EnhancedComment?> GetCommentByIdAsync(Guid commentId, CancellationToken cancellationToken = default);
}

public class GetCommentRepository(IEnhancedCommentsPlugin plugin) : IGetCommentRepository
{
    public Task<EnhancedComment?> GetCommentByIdAsync(Guid commentId, CancellationToken cancellationToken = default) =>
        plugin.GetCommentByIdAsync(commentId);
}
