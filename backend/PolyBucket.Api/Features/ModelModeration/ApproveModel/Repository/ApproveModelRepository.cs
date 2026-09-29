using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.ModelModeration.Domain;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.ModelModeration.ApproveModel.Repository;

public class ApproveModelRepository(PolyBucketDbContext context) : IApproveModelRepository
{
    private readonly PolyBucketDbContext _context = context;

    public async Task<Model?> GetModelAsync(Guid modelId, CancellationToken cancellationToken = default)
    {
        return await _context.Models
            .FirstOrDefaultAsync(m => m.Id == modelId && m.DeletedAt == null, cancellationToken);
    }

    public async Task<ModelModerationRecord?> GetModerationRecordAsync(Guid modelId, CancellationToken cancellationToken = default)
    {
        return await _context.ModelModerationRecords.FirstOrDefaultAsync(r => r.ModelId == modelId, cancellationToken);
    }

    public async Task SaveAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
