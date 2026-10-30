using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PolyBucket.Api.Features.Comments.Plugins;
using PolyBucket.Database.Entities.Comments;

namespace PolyBucket.Api.Features.Comments.GetCommentsForTarget.Domain;

public interface ICommentResponseMapper
{
    Task<List<CommentResponse>> MapAsync(
        IReadOnlyList<EnhancedComment> comments,
        Guid? currentUserId,
        bool isAdmin,
        bool includeReplies,
        CancellationToken cancellationToken = default);
}
