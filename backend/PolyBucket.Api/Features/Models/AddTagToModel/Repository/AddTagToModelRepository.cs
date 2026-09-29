using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Models.AddTagToModel.Domain;

namespace PolyBucket.Api.Features.Models.AddTagToModel.Repository;

public class AddTagToModelRepository : IAddTagToModelRepository
{
    private readonly PolyBucketDbContext _dbContext;

    public AddTagToModelRepository(PolyBucketDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Model?> GetModelAsync(Guid modelId, CancellationToken cancellationToken)
    {
        return await _dbContext.Models
            .Include(m => m.Tags)
            .FirstOrDefaultAsync(m => m.Id == modelId, cancellationToken);
    }

    public async Task<Tag?> GetActiveTagByNameAsync(string name, CancellationToken cancellationToken)
    {
        var normalized = name.ToLower();
        return await _dbContext.Tags
            .FirstOrDefaultAsync(
                t => t.DeletedAt == null && t.Name.ToLower() == normalized,
                cancellationToken);
    }

    public void AddTag(Tag tag)
    {
        _dbContext.Tags.Add(tag);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
