using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Comments.Domain;

namespace PolyBucket.Api.Features.Comments.Repository;

public class CommentReactionRepository(PolyBucketDbContext context, TimeProvider timeProvider) : ICommentReactionRepository
{
    public Task<bool> IsReactableAsync(Guid commentId, CancellationToken cancellationToken = default) =>
        context.EnhancedComments.AnyAsync(c => c.Id == commentId && !c.IsHidden, cancellationToken);

    public async Task<Guid?> GetAuthorIdAsync(Guid commentId, CancellationToken cancellationToken = default)
    {
        return await context.EnhancedComments
            .AsNoTracking()
            .Where(c => c.Id == commentId && !c.IsHidden)
            .Select(c => (Guid?)c.AuthorId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<CommentReactionType?> GetUserReactionAsync(Guid commentId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await context.CommentReactions
            .Where(r => r.CommentId == commentId && r.UserId == userId)
            .Select(r => (CommentReactionType?)r.Type)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyDictionary<Guid, CommentReactionType>> GetUserReactionsAsync(IReadOnlyCollection<Guid> commentIds, Guid userId, CancellationToken cancellationToken = default)
    {
        if (commentIds.Count == 0)
        {
            return new Dictionary<Guid, CommentReactionType>();
        }

        return await context.CommentReactions
            .Where(r => r.UserId == userId && commentIds.Contains(r.CommentId))
            .ToDictionaryAsync(r => r.CommentId, r => r.Type, cancellationToken);
    }

    public async Task<(int Likes, int Dislikes)> GetCountsAsync(Guid commentId, CancellationToken cancellationToken = default)
    {
        var counts = await context.EnhancedComments
            .AsNoTracking()
            .Where(c => c.Id == commentId)
            .Select(c => new { c.Likes, c.Dislikes })
            .FirstOrDefaultAsync(cancellationToken);
        return counts == null ? (0, 0) : (counts.Likes, counts.Dislikes);
    }

    public async Task<bool> TryAddAsync(Guid commentId, Guid userId, CommentReactionType type, CancellationToken cancellationToken = default)
    {
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        var reaction = new CommentReaction
        {
            Id = Guid.NewGuid(),
            CommentId = commentId,
            UserId = userId,
            Type = type,
            CreatedAt = timeProvider.GetUtcNow().UtcDateTime
        };
        context.CommentReactions.Add(reaction);

        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            context.Entry(reaction).State = EntityState.Detached;
            await transaction.RollbackAsync(cancellationToken);
            return false;
        }

        await AdjustCountersAsync(commentId, type == CommentReactionType.Like ? 1 : 0, type == CommentReactionType.Dislike ? 1 : 0, cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return true;
    }

    public async Task<bool> TrySwitchAsync(Guid commentId, Guid userId, CommentReactionType from, CommentReactionType to, CancellationToken cancellationToken = default)
    {
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        var switched = await context.CommentReactions
            .Where(r => r.CommentId == commentId && r.UserId == userId && r.Type == from)
            .ExecuteUpdateAsync(s => s.SetProperty(r => r.Type, to), cancellationToken);

        if (switched == 0)
        {
            await transaction.RollbackAsync(cancellationToken);
            return false;
        }

        var likeDelta = to == CommentReactionType.Like ? 1 : -1;
        await AdjustCountersAsync(commentId, likeDelta, -likeDelta, cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return true;
    }

    public async Task<bool> TryRemoveAsync(Guid commentId, Guid userId, CommentReactionType type, CancellationToken cancellationToken = default)
    {
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        var removed = await context.CommentReactions
            .Where(r => r.CommentId == commentId && r.UserId == userId && r.Type == type)
            .ExecuteDeleteAsync(cancellationToken);

        if (removed == 0)
        {
            await transaction.RollbackAsync(cancellationToken);
            return false;
        }

        await AdjustCountersAsync(commentId, type == CommentReactionType.Like ? -1 : 0, type == CommentReactionType.Dislike ? -1 : 0, cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return true;
    }

    private Task<int> AdjustCountersAsync(Guid commentId, int likeDelta, int dislikeDelta, CancellationToken cancellationToken) =>
        context.EnhancedComments
            .Where(c => c.Id == commentId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(c => c.Likes, c => c.Likes + likeDelta < 0 ? 0 : c.Likes + likeDelta)
                .SetProperty(c => c.Dislikes, c => c.Dislikes + dislikeDelta < 0 ? 0 : c.Dislikes + dislikeDelta),
                cancellationToken);
}
