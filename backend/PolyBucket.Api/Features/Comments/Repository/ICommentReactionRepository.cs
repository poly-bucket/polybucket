using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PolyBucket.Api.Features.Comments.Domain;

namespace PolyBucket.Api.Features.Comments.Repository;

public interface ICommentReactionRepository
{
    Task<bool> IsReactableAsync(Guid commentId, CancellationToken cancellationToken = default);
    Task<Guid?> GetAuthorIdAsync(Guid commentId, CancellationToken cancellationToken = default);
    Task<CommentReactionType?> GetUserReactionAsync(Guid commentId, Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyDictionary<Guid, CommentReactionType>> GetUserReactionsAsync(IReadOnlyCollection<Guid> commentIds, Guid userId, CancellationToken cancellationToken = default);
    Task<(int Likes, int Dislikes)> GetCountsAsync(Guid commentId, CancellationToken cancellationToken = default);
    Task<bool> TryAddAsync(Guid commentId, Guid userId, CommentReactionType type, CancellationToken cancellationToken = default);
    Task<bool> TrySwitchAsync(Guid commentId, Guid userId, CommentReactionType from, CommentReactionType to, CancellationToken cancellationToken = default);
    Task<bool> TryRemoveAsync(Guid commentId, Guid userId, CommentReactionType type, CancellationToken cancellationToken = default);
}
