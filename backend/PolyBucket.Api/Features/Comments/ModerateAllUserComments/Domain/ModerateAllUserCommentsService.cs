using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using PolyBucket.Api.Features.Comments.ModerateAllUserComments.Repository;

namespace PolyBucket.Api.Features.Comments.ModerateAllUserComments.Domain;

public interface IModerateAllUserCommentsService
{
    Task<bool> ModerateAllAsync(Guid targetUserId, Guid moderatorId, string reason, CancellationToken cancellationToken = default);
}

public class ModerateAllUserCommentsService(IModerateAllUserCommentsRepository repository) : IModerateAllUserCommentsService
{
    public Task<bool> ModerateAllAsync(Guid targetUserId, Guid moderatorId, string reason, CancellationToken cancellationToken = default) =>
        repository.ModerateAllCommentsForUserAsync(targetUserId, moderatorId, reason, cancellationToken);
}
