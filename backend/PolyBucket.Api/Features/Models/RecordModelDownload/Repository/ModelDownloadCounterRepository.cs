using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Models.RecordModelDownload.Domain;

namespace PolyBucket.Api.Features.Models.RecordModelDownload.Repository;

public class ModelDownloadCounterRepository(PolyBucketDbContext context) : IModelDownloadCounterRepository
{
    public async Task<Model?> GetModelAsync(Guid modelId, CancellationToken cancellationToken = default)
    {
        return await context.Models
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == modelId && m.DeletedAt == null, cancellationToken);
    }

    public async Task<(int Downloads, bool Counted)> TryRecordDownloadAsync(
        Guid modelId,
        string viewerKey,
        DateTime nowUtc,
        DateTime dedupThresholdUtc,
        CancellationToken cancellationToken = default)
    {
        var model = await context.Models
            .FirstOrDefaultAsync(m => m.Id == modelId && m.DeletedAt == null, cancellationToken);
        if (model == null)
        {
            return (0, false);
        }

        var dedup = await context.ModelDownloadDedups
            .FirstOrDefaultAsync(d => d.ModelId == modelId && d.ViewerKey == viewerKey, cancellationToken);

        if (dedup != null && dedup.LastCountedAt > dedupThresholdUtc)
        {
            return (model.Downloads, false);
        }

        if (dedup == null)
        {
            context.ModelDownloadDedups.Add(new ModelDownloadDedup
            {
                Id = Guid.NewGuid(),
                ModelId = modelId,
                ViewerKey = viewerKey,
                LastCountedAt = nowUtc
            });
        }
        else
        {
            dedup.LastCountedAt = nowUtc;
        }

        model.Downloads++;
        await context.SaveChangesAsync(cancellationToken);
        return (model.Downloads, true);
    }
}
