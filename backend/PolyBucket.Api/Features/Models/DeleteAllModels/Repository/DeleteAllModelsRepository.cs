using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Data;
using ModelEntity = PolyBucket.Api.Common.Models.Model;

namespace PolyBucket.Api.Features.Models.DeleteAllModels.Repository;

public class DeleteAllModelsRepository(PolyBucketDbContext context) : IDeleteAllModelsRepository
{
    private readonly PolyBucketDbContext _context = context;

    public async Task<int> DeleteAllModelsAndReturnCountAsync(CancellationToken cancellationToken = default)
    {
        var modelCount = await _context.Models.CountAsync(cancellationToken);
        if (modelCount == 0)
        {
            return 0;
        }

        var models = await _context.Set<ModelEntity>().ToListAsync(cancellationToken);
        _context.Models.RemoveRange(models);

        var orphanedComments = await _context.Comments
            .Where(c => !_context.Set<ModelEntity>().Any(m => m.Id == c.Model.Id))
            .ToListAsync(cancellationToken);
        _context.Comments.RemoveRange(orphanedComments);

        var orphanedLikes = await _context.Likes
            .Where(l => !_context.Set<ModelEntity>().Any(m => m.Id == l.ModelId))
            .ToListAsync(cancellationToken);
        _context.Likes.RemoveRange(orphanedLikes);

        await _context.SaveChangesAsync(cancellationToken);
        return modelCount;
    }
}
