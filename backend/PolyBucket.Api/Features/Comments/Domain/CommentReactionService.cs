using System;
using System.Threading;
using System.Threading.Tasks;
using PolyBucket.Api.Features.Comments.Repository;

namespace PolyBucket.Api.Features.Comments.Domain;

public class CommentReactionService(ICommentReactionRepository repository) : ICommentReactionService
{
    public async Task<CommentReactionOutcome> ReactAsync(Guid commentId, Guid userId, CommentReactionType type, CancellationToken cancellationToken = default)
    {
        if (!await repository.IsReactableAsync(commentId, cancellationToken))
        {
            return CommentReactionOutcome.NotFound;
        }

        var authorId = await repository.GetAuthorIdAsync(commentId, cancellationToken);
        if (authorId == userId)
        {
            return CommentReactionOutcome.Forbidden;
        }

        var existing = await repository.GetUserReactionAsync(commentId, userId, cancellationToken);
        var applied = existing switch
        {
            null => await repository.TryAddAsync(commentId, userId, type, cancellationToken),
            var current when current == type => false,
            var current => await repository.TrySwitchAsync(commentId, userId, current.Value, type, cancellationToken)
        };

        return await BuildOutcomeAsync(commentId, userId, applied, cancellationToken);
    }

    public async Task<CommentReactionOutcome> RemoveReactionAsync(Guid commentId, Guid userId, CommentReactionType type, CancellationToken cancellationToken = default)
    {
        if (!await repository.IsReactableAsync(commentId, cancellationToken))
        {
            return CommentReactionOutcome.NotFound;
        }

        var authorId = await repository.GetAuthorIdAsync(commentId, cancellationToken);
        if (authorId == userId)
        {
            return CommentReactionOutcome.Forbidden;
        }

        var existing = await repository.GetUserReactionAsync(commentId, userId, cancellationToken);
        var applied = existing == type && await repository.TryRemoveAsync(commentId, userId, type, cancellationToken);

        return await BuildOutcomeAsync(commentId, userId, applied, cancellationToken);
    }

    private async Task<CommentReactionOutcome> BuildOutcomeAsync(Guid commentId, Guid userId, bool applied, CancellationToken cancellationToken)
    {
        var (likes, dislikes) = await repository.GetCountsAsync(commentId, cancellationToken);
        var current = await repository.GetUserReactionAsync(commentId, userId, cancellationToken);
        return new CommentReactionOutcome(applied ? CommentReactionChange.Applied : CommentReactionChange.Unchanged, likes, dislikes, current);
    }
}
