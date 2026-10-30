using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using PolyBucket.Api.Features.Comments.Plugins;
using PolyBucket.Database.Entities.Comments;

namespace PolyBucket.Api.Features.Comments.CreateComment.Repository;

public interface ICreateCommentRepository
{
    Task<EnhancedComment> AddCommentAsync(CommentTarget target, Guid authorId, string content, Guid? parentCommentId, CancellationToken cancellationToken = default);
}

public class CreateCommentRepository(IEnhancedCommentsPlugin plugin) : ICreateCommentRepository
{
    public Task<EnhancedComment> AddCommentAsync(CommentTarget target, Guid authorId, string content, Guid? parentCommentId, CancellationToken cancellationToken = default) =>
        plugin.AddCommentAsync(target, authorId, content, parentCommentId);
}
