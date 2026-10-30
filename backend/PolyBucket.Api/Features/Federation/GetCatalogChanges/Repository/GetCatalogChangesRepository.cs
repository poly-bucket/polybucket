using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Features.Federation.GetCatalogChanges.Http;

namespace PolyBucket.Api.Features.Federation.GetCatalogChanges.Repository;

public class GetCatalogChangesRepository(PolyBucketDbContext context) : IGetCatalogChangesRepository
{
    public async Task<CatalogChangesResponse> GetChangesAsync(DateTime since, CancellationToken cancellationToken = default)
    {
        var newModels = await context.Models
            .Where(m => !m.IsFederated && m.CreatedAt > since)
            .Select(m => new CatalogChangeDto
            {
                Id = m.Id.ToString(),
                Action = "created",
                UpdatedAt = m.CreatedAt
            })
            .ToListAsync(cancellationToken);

        var updatedModels = await context.Models
            .Where(m => !m.IsFederated && m.UpdatedAt.HasValue && m.UpdatedAt > since)
            .Select(m => new CatalogChangeDto
            {
                Id = m.Id.ToString(),
                Action = "updated",
                UpdatedAt = m.UpdatedAt!.Value
            })
            .ToListAsync(cancellationToken);

        var allChanges = newModels.Concat(updatedModels).OrderBy(c => c.UpdatedAt).ToList();
        return new CatalogChangesResponse
        {
            NewModels = newModels.Count,
            UpdatedModels = updatedModels.Count,
            DeletedModels = 0,
            Models = allChanges
        };
    }
}
