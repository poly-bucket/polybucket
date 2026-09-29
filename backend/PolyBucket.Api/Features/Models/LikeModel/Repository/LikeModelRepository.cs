using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Models.LikeModel.Domain;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Models.LikeModel.Repository;

public class LikeModelRepository(PolyBucketDbContext dbContext) : ILikeModelRepository
{
    public Task<Model?> GetModelByIdAsync(Guid modelId, CancellationToken cancellationToken)
    {
        return dbContext.Models
            .FirstOrDefaultAsync(m => m.Id == modelId && m.DeletedAt == null, cancellationToken);
    }

    public Task<Like?> FindLikeAsync(Guid modelId, Guid userId, CancellationToken cancellationToken)
    {
        return dbContext.Likes
            .FirstOrDefaultAsync(l => l.ModelId == modelId && l.UserId == userId, cancellationToken);
    }

    public async Task<bool> IsModelLikesEnabledAsync(CancellationToken cancellationToken)
    {
        var settings = await dbContext.ModelSettings.FirstOrDefaultAsync(cancellationToken);
        return settings?.EnableModelLikes ?? true;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }

    public void AddLike(Like like)
    {
        dbContext.Likes.Add(like);
    }
}
