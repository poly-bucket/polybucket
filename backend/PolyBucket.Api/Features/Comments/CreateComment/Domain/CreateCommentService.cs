using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using PolyBucket.Api.Features.Comments.CreateComment.Repository;
using PolyBucket.Api.Features.Comments.GetCommentsForTarget.Domain;
using PolyBucket.Api.Features.Comments.Plugins;
using PolyBucket.Database.Entities.Comments;

namespace PolyBucket.Api.Features.Comments.CreateComment.Domain;

public interface ICreateCommentService
{
    Task<CreateCommentResult> CreateAsync(Guid userId, CommentTarget target, string content, Guid? parentCommentId, bool isAdmin, CancellationToken cancellationToken = default);
}

public sealed record CreateCommentResult(CommentResponse? Comment, string? ErrorMessage);

public class CreateCommentService(ICreateCommentRepository repository, ICommentResponseMapper mapper) : ICreateCommentService
{
    public async Task<CreateCommentResult> CreateAsync(Guid userId, CommentTarget target, string content, Guid? parentCommentId, bool isAdmin, CancellationToken cancellationToken = default)
    {
        try
        {
            var comment = await repository.AddCommentAsync(target, userId, content, parentCommentId, cancellationToken);
            var mapped = await mapper.MapAsync(new[] { comment }, userId, isAdmin, includeReplies: false, cancellationToken);
            return new CreateCommentResult(mapped[0], null);
        }
        catch (InvalidOperationException ex)
        {
            return new CreateCommentResult(null, ex.Message);
        }
    }
}
