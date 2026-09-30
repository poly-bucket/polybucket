using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.ModelModeration.Domain;
using PolyBucket.Api.Features.Models.CreateModelVersion.Domain;
using PolyBucket.Api.Features.Models.GetModelVersions.Domain;

namespace PolyBucket.Api.Features.Models.GetModelVersions.Repository;

public class GetModelVersionsRepository(PolyBucketDbContext context) : IGetModelVersionsRepository
{
    public Task<ModelVersionsAccessInfo?> GetAccessInfoAsync(Guid modelId, CancellationToken cancellationToken)
    {
        return context.Models
            .AsNoTracking()
            .Where(m => m.Id == modelId && m.DeletedAt == null)
            .Select(m => new ModelVersionsAccessInfo(
                m.AuthorId,
                m.Privacy,
                m.IsPublic,
                !context.ModelModerationRecords.Any(r => r.ModelId == m.Id)
                    || context.ModelModerationRecords.Any(r => r.ModelId == m.Id && r.Status == ModelModerationStatus.Approved)))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<List<ModelVersion>> GetVersionsAsync(Guid modelId, CancellationToken cancellationToken)
    {
        return context.ModelVersions
            .AsNoTracking()
            .Include(v => v.Files.Where(f => f.DeletedAt == null))
            .Where(v => v.ModelId == modelId && v.DeletedAt == null)
            .OrderByDescending(v => v.VersionNumber)
            .ThenByDescending(v => v.CreatedAt)
            .ThenBy(v => v.Id)
            .ToListAsync(cancellationToken);
    }
}
