using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using PolyBucket.Api.Features.Comments.Plugins;

namespace PolyBucket.Api.Features.Comments.ModerateAllUserComments.Repository;

public interface IModerateAllUserCommentsRepository
{
    Task<bool> ModerateAllCommentsForUserAsync(Guid targetUserId, Guid moderatorId, string reason, CancellationToken cancellationToken = default);
}

public class ModerateAllUserCommentsRepository(IEnhancedCommentsPlugin plugin) : IModerateAllUserCommentsRepository
{
    public Task<bool> ModerateAllCommentsForUserAsync(Guid targetUserId, Guid moderatorId, string reason, CancellationToken cancellationToken = default) =>
        plugin.ModerateAllCommentsForUserAsync(targetUserId, moderatorId, reason);
}
