using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Models.LikeModel.Domain;
using PolyBucket.Api.Features.Models.ModelReactions.Domain;

namespace PolyBucket.Api.Features.Models.ModelReactions.Repository;

public class ModelReactionRepository(PolyBucketDbContext context, TimeProvider timeProvider) : IModelReactionRepository
{
    public Task<Model?> GetModelByIdAsync(Guid modelId, CancellationToken cancellationToken = default) =>
        context.Models.FirstOrDefaultAsync(m => m.Id == modelId && m.DeletedAt == null, cancellationToken);

    public async Task<bool> IsReactionsEnabledAsync(CancellationToken cancellationToken = default)
    {
        var settings = await context.ModelSettings.AsNoTracking().FirstOrDefaultAsync(cancellationToken);
        return settings?.EnableModelLikes ?? true;
    }

    public async Task<ModelReactionType?> GetUserReactionAsync(Guid modelId, Guid userId, CancellationToken cancellationToken = default)
    {
        var row = await context.Likes
            .AsNoTracking()
            .Where(l => l.ModelId == modelId && l.UserId == userId && l.DeletedAt == null)
            .Select(l => (ModelReactionType?)l.Type)
            .FirstOrDefaultAsync(cancellationToken);
        return row;
    }

    public async Task<(int Likes, int Dislikes)> GetCountsAsync(Guid modelId, CancellationToken cancellationToken = default)
    {
        var counts = await context.Models
            .AsNoTracking()
            .Where(m => m.Id == modelId)
            .Select(m => new { m.Likes, m.Dislikes })
            .FirstOrDefaultAsync(cancellationToken);
        return counts == null ? (0, 0) : (counts.Likes, counts.Dislikes);
    }

    public async Task<bool> TryAddAsync(Guid modelId, Guid userId, ModelReactionType type, CancellationToken cancellationToken = default)
    {
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var existing = await context.Likes
            .FirstOrDefaultAsync(l => l.ModelId == modelId && l.UserId == userId, cancellationToken);

        if (existing != null)
        {
            if (existing.DeletedAt == null)
            {
                await transaction.RollbackAsync(cancellationToken);
                return false;
            }

            existing.DeletedAt = null;
            existing.DeletedById = null;
            existing.Type = type;
            existing.UpdatedAt = now;
            existing.UpdatedById = userId;
        }
        else
        {
            context.Likes.Add(new Like
            {
                Id = Guid.NewGuid(),
                ModelId = modelId,
                UserId = userId,
                Type = type,
                CreatedAt = now,
                CreatedById = userId,
                UpdatedAt = now,
                UpdatedById = userId
            });
        }

        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            await transaction.RollbackAsync(cancellationToken);
            return false;
        }

        await AdjustCountersAsync(modelId, type == ModelReactionType.Like ? 1 : 0, type == ModelReactionType.Dislike ? 1 : 0, now, userId, cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return true;
    }

    public async Task<bool> TrySwitchAsync(Guid modelId, Guid userId, ModelReactionType from, ModelReactionType to, CancellationToken cancellationToken = default)
    {
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var switched = await context.Likes
            .Where(l => l.ModelId == modelId && l.UserId == userId && l.DeletedAt == null && l.Type == from)
            .ExecuteUpdateAsync(s => s
                .SetProperty(l => l.Type, to)
                .SetProperty(l => l.UpdatedAt, now)
                .SetProperty(l => l.UpdatedById, userId),
                cancellationToken);

        if (switched == 0)
        {
            await transaction.RollbackAsync(cancellationToken);
            return false;
        }

        var likeDelta = to == ModelReactionType.Like ? 1 : -1;
        await AdjustCountersAsync(modelId, likeDelta, -likeDelta, now, userId, cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return true;
    }

    public async Task<bool> TryRemoveAsync(Guid modelId, Guid userId, ModelReactionType type, CancellationToken cancellationToken = default)
    {
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var row = await context.Likes
            .FirstOrDefaultAsync(l => l.ModelId == modelId && l.UserId == userId && l.DeletedAt == null && l.Type == type, cancellationToken);

        if (row == null)
        {
            await transaction.RollbackAsync(cancellationToken);
            return false;
        }

        row.DeletedAt = now;
        row.DeletedById = userId;
        row.UpdatedAt = now;
        row.UpdatedById = userId;
        await context.SaveChangesAsync(cancellationToken);

        await AdjustCountersAsync(
            modelId,
            type == ModelReactionType.Like ? -1 : 0,
            type == ModelReactionType.Dislike ? -1 : 0,
            now,
            userId,
            cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return true;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        context.SaveChangesAsync(cancellationToken);

    private Task<int> AdjustCountersAsync(
        Guid modelId,
        int likeDelta,
        int dislikeDelta,
        DateTime updatedAt,
        Guid updatedById,
        CancellationToken cancellationToken) =>
        context.Models
            .Where(m => m.Id == modelId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(m => m.Likes, m => m.Likes + likeDelta < 0 ? 0 : m.Likes + likeDelta)
                .SetProperty(m => m.Dislikes, m => m.Dislikes + dislikeDelta < 0 ? 0 : m.Dislikes + dislikeDelta)
                .SetProperty(m => m.UpdatedAt, updatedAt)
                .SetProperty(m => m.UpdatedById, updatedById),
                cancellationToken);
}
