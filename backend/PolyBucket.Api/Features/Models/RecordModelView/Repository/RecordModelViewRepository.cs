using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Models.RecordModelView.Domain;

namespace PolyBucket.Api.Features.Models.RecordModelView.Repository;

public class RecordModelViewRepository(PolyBucketDbContext context) : IRecordModelViewRepository
{
    public async Task<Model?> GetModelForViewAsync(Guid modelId, CancellationToken cancellationToken = default)
    {
        return await context.Models
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == modelId && m.DeletedAt == null, cancellationToken);
    }

    public async Task<(int Views, bool Counted)> TryRecordViewAsync(
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

        var dedup = await context.ModelViewDedups
            .FirstOrDefaultAsync(d => d.ModelId == modelId && d.ViewerKey == viewerKey, cancellationToken);

        if (dedup != null && dedup.LastCountedAt > dedupThresholdUtc)
        {
            return (model.Views, false);
        }

        if (dedup == null)
        {
            context.ModelViewDedups.Add(new ModelViewDedup
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

        model.Views++;
        await context.SaveChangesAsync(cancellationToken);
        return (model.Views, true);
    }
}
