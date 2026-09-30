using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Models.GenerateModelPreview.Domain;

namespace PolyBucket.Api.Features.Models.GenerateModelPreview.Repository;

public class ModelPreviewQueueRepository(PolyBucketDbContext context) : IModelPreviewQueueRepository
{
    private const int MaxErrorLength = 2000;

    public Task<ModelPreview?> GetAsync(Guid modelId, string size, CancellationToken cancellationToken)
    {
        return context.ModelPreviews.FirstOrDefaultAsync(p => p.ModelId == modelId && p.Size == size, cancellationToken);
    }

    public Task<Guid?> GetModelAuthorIdAsync(Guid modelId, CancellationToken cancellationToken)
    {
        return context.Models
            .AsNoTracking()
            .Where(m => m.Id == modelId && m.DeletedAt == null)
            .Select(m => (Guid?)m.AuthorId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> IsAutoGenerateEnabledAsync(CancellationToken cancellationToken)
    {
        var enabled = await context.ModelSettings
            .AsNoTracking()
            .Select(s => (bool?)s.AutoGenerateModelPreviews)
            .FirstOrDefaultAsync(cancellationToken);
        return enabled ?? true;
    }

    public async Task<bool> TryAddAsync(ModelPreview preview, CancellationToken cancellationToken)
    {
        context.ModelPreviews.Add(preview);
        try
        {
            await context.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            context.Entry(preview).State = EntityState.Detached;
            return false;
        }
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ModelPreview>> ClaimBatchAsync(int batchSize, TimeSpan lockDuration, DateTime now, CancellationToken cancellationToken)
    {
        var lockedUntil = now.Add(lockDuration);
        var pending = (int)PreviewStatus.Pending;
        var generating = (int)PreviewStatus.Generating;

        return await context.ModelPreviews
            .FromSqlInterpolated($@"
                UPDATE ""ModelPreviews""
                SET ""Status"" = {generating}, ""LockedUntil"" = {lockedUntil}, ""Attempts"" = ""Attempts"" + 1
                WHERE ""Id"" IN (
                    SELECT ""Id"" FROM ""ModelPreviews""
                    WHERE (""Status"" = {pending} AND (""NextAttemptAt"" IS NULL OR ""NextAttemptAt"" <= {now}))
                       OR (""Status"" = {generating} AND ""LockedUntil"" < {now})
                    ORDER BY ""NextAttemptAt"" NULLS FIRST, ""CreatedAt""
                    LIMIT {batchSize}
                    FOR UPDATE SKIP LOCKED)
                RETURNING *")
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ModelPreviewSourceFile>> GetCandidateFilesAsync(Guid modelId, CancellationToken cancellationToken)
    {
        var latestVersionFiles = await context.ModelVersions
            .AsNoTracking()
            .Where(v => v.ModelId == modelId && v.DeletedAt == null)
            .OrderByDescending(v => v.VersionNumber)
            .Select(v => v.Files
                .Where(f => f.DeletedAt == null)
                .OrderBy(f => f.Name)
                .Select(f => new ModelPreviewSourceFile(f.Name, f.Path))
                .ToList())
            .FirstOrDefaultAsync(cancellationToken);

        var modelFiles = await context.Models
            .AsNoTracking()
            .Where(m => m.Id == modelId)
            .SelectMany(m => m.Files)
            .Where(f => f.DeletedAt == null)
            .OrderBy(f => f.CreatedAt)
            .ThenBy(f => f.Name)
            .Select(f => new ModelPreviewSourceFile(f.Name, f.Path))
            .ToListAsync(cancellationToken);

        return (latestVersionFiles ?? []).Concat(modelFiles).ToList();
    }

    public Task MarkCompletedAsync(Guid previewId, ModelPreview result, DateTime now, CancellationToken cancellationToken)
    {
        return context.ModelPreviews
            .Where(p => p.Id == previewId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(p => p.Status, PreviewStatus.Completed)
                .SetProperty(p => p.PreviewUrl, result.PreviewUrl)
                .SetProperty(p => p.StorageKey, result.StorageKey)
                .SetProperty(p => p.Width, result.Width)
                .SetProperty(p => p.Height, result.Height)
                .SetProperty(p => p.FileSizeBytes, result.FileSizeBytes)
                .SetProperty(p => p.GeneratedAt, result.GeneratedAt ?? now)
                .SetProperty(p => p.ErrorMessage, (string?)null)
                .SetProperty(p => p.LockedUntil, (DateTime?)null)
                .SetProperty(p => p.NextAttemptAt, (DateTime?)null)
                .SetProperty(p => p.UpdatedAt, now), cancellationToken);
    }

    public Task MarkRetryAsync(Guid previewId, string error, DateTime nextAttemptAt, CancellationToken cancellationToken)
    {
        var truncated = Truncate(error);
        return context.ModelPreviews
            .Where(p => p.Id == previewId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(p => p.Status, PreviewStatus.Pending)
                .SetProperty(p => p.ErrorMessage, truncated)
                .SetProperty(p => p.NextAttemptAt, nextAttemptAt)
                .SetProperty(p => p.LockedUntil, (DateTime?)null), cancellationToken);
    }

    public Task MarkFailedAsync(Guid previewId, string error, DateTime now, CancellationToken cancellationToken)
    {
        var truncated = Truncate(error);
        return context.ModelPreviews
            .Where(p => p.Id == previewId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(p => p.Status, PreviewStatus.Failed)
                .SetProperty(p => p.ErrorMessage, truncated)
                .SetProperty(p => p.LockedUntil, (DateTime?)null)
                .SetProperty(p => p.NextAttemptAt, (DateTime?)null)
                .SetProperty(p => p.UpdatedAt, now), cancellationToken);
    }

    private static string Truncate(string value)
    {
        return value.Length <= MaxErrorLength ? value : value[..MaxErrorLength];
    }
}
