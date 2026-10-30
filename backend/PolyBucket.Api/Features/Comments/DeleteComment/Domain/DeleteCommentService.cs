using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using PolyBucket.Api.Features.Comments.DeleteComment.Repository;

namespace PolyBucket.Api.Features.Comments.DeleteComment.Domain;

public interface IDeleteCommentService
{
    Task<bool> DeleteAsync(Guid commentId, Guid userId, bool isAdmin, CancellationToken cancellationToken = default);
}

public class DeleteCommentService(IDeleteCommentRepository repository) : IDeleteCommentService
{
    public Task<bool> DeleteAsync(Guid commentId, Guid userId, bool isAdmin, CancellationToken cancellationToken = default) =>
        repository.DeleteCommentAsync(commentId, userId, isAdmin, cancellationToken);
}
