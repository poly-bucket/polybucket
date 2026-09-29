using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Data;

namespace PolyBucket.Api.Features.Models.RemoveCategoryFromModel.Repository;

public class RemoveCategoryFromModelRepository : IRemoveCategoryFromModelRepository
{
    private readonly PolyBucketDbContext _dbContext;

    public RemoveCategoryFromModelRepository(PolyBucketDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Model?> GetModelAsync(Guid modelId, CancellationToken cancellationToken)
    {
        return await _dbContext.Models
            .Include(m => m.Categories)
            .FirstOrDefaultAsync(m => m.Id == modelId, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
