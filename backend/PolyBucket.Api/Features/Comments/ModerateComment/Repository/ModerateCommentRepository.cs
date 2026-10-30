using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using PolyBucket.Api.Features.Comments.Plugins;

namespace PolyBucket.Api.Features.Comments.ModerateComment.Repository;

public interface IModerateCommentRepository
{
    Task<bool> ModerateCommentAsync(Guid commentId, Guid moderatorId, string reason, CancellationToken cancellationToken = default);
}

public class ModerateCommentRepository(IEnhancedCommentsPlugin plugin) : IModerateCommentRepository
{
    public Task<bool> ModerateCommentAsync(Guid commentId, Guid moderatorId, string reason, CancellationToken cancellationToken = default) =>
        plugin.ModerateCommentAsync(commentId, moderatorId, reason);
}
