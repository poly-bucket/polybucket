using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using PolyBucket.Api.Features.Comments.ModerateComment.Repository;

namespace PolyBucket.Api.Features.Comments.ModerateComment.Domain;

public interface IModerateCommentService
{
    Task<bool> ModerateAsync(Guid commentId, Guid moderatorId, string reason, CancellationToken cancellationToken = default);
}

public class ModerateCommentService(IModerateCommentRepository repository) : IModerateCommentService
{
    public Task<bool> ModerateAsync(Guid commentId, Guid moderatorId, string reason, CancellationToken cancellationToken = default) =>
        repository.ModerateCommentAsync(commentId, moderatorId, reason, cancellationToken);
}
