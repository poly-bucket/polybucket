using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Comments.Domain;

public interface ICommentResponseMapper
{
    Task<List<CommentResponse>> MapAsync(
        IReadOnlyList<EnhancedComment> comments,
        Guid? currentUserId,
        bool isAdmin,
        bool includeReplies,
        CancellationToken cancellationToken = default);
}
