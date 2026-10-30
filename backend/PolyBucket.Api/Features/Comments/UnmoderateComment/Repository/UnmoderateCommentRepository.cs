using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using PolyBucket.Api.Features.Comments.Plugins;

namespace PolyBucket.Api.Features.Comments.UnmoderateComment.Repository;

public interface IUnmoderateCommentRepository
{
    Task<bool> UnmoderateCommentAsync(Guid commentId, Guid moderatorId, CancellationToken cancellationToken = default);
}

public class UnmoderateCommentRepository(IEnhancedCommentsPlugin plugin) : IUnmoderateCommentRepository
{
    public Task<bool> UnmoderateCommentAsync(Guid commentId, Guid moderatorId, CancellationToken cancellationToken = default) =>
        plugin.UnmoderateCommentAsync(commentId, moderatorId);
}
