using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using PolyBucket.Api.Features.Comments.UnmoderateComment.Repository;

namespace PolyBucket.Api.Features.Comments.UnmoderateComment.Domain;

public interface IUnmoderateCommentService
{
    Task<bool> UnmoderateAsync(Guid commentId, Guid moderatorId, CancellationToken cancellationToken = default);
}

public class UnmoderateCommentService(IUnmoderateCommentRepository repository) : IUnmoderateCommentService
{
    public Task<bool> UnmoderateAsync(Guid commentId, Guid moderatorId, CancellationToken cancellationToken = default) =>
        repository.UnmoderateCommentAsync(commentId, moderatorId, cancellationToken);
}
