using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Models.AddCategoryToModel.Domain;

namespace PolyBucket.Api.Features.Models.AddCategoryToModel.Repository;

public class AddCategoryToModelRepository : IAddCategoryToModelRepository
{
    private readonly PolyBucketDbContext _dbContext;

    public AddCategoryToModelRepository(PolyBucketDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Model?> GetModelAsync(Guid modelId, CancellationToken cancellationToken)
    {
        return await _dbContext.Models
            .Include(m => m.Categories)
            .FirstOrDefaultAsync(m => m.Id == modelId, cancellationToken);
    }

    public async Task<Category?> GetActiveCategoryAsync(Guid categoryId, CancellationToken cancellationToken)
    {
        return await _dbContext.Categories
            .FirstOrDefaultAsync(c => c.Id == categoryId && c.DeletedAt == null, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
