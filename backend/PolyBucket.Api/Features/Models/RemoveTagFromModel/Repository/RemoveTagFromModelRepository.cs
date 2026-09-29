using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Data;

namespace PolyBucket.Api.Features.Models.RemoveTagFromModel.Repository;

public class RemoveTagFromModelRepository : IRemoveTagFromModelRepository
{
    private readonly PolyBucketDbContext _dbContext;

    public RemoveTagFromModelRepository(PolyBucketDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Model?> GetModelAsync(Guid modelId, CancellationToken cancellationToken)
    {
        return await _dbContext.Models
            .Include(m => m.Tags)
            .FirstOrDefaultAsync(m => m.Id == modelId, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
