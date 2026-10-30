using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Features.Federation.GetCatalog.Http;

namespace PolyBucket.Api.Features.Federation.GetCatalog.Repository;

public class GetCatalogRepository(PolyBucketDbContext context) : IGetCatalogRepository
{
    public async Task<(int TotalCount, List<CatalogModelDto> Models)> GetCatalogPageAsync(int page, int pageSize, DateTime? since, CancellationToken cancellationToken = default)
    {
        var query = context.Models
            .Where(m => !m.IsFederated)
            .Include(m => m.Author)
            .AsQueryable();

        if (since.HasValue)
            query = query.Where(m => m.UpdatedAt > since.Value || m.CreatedAt > since.Value);

        var totalCount = await query.CountAsync(cancellationToken);
        var models = await query
            .OrderByDescending(m => m.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(m => new CatalogModelDto
            {
                Id = m.Id.ToString(),
                Name = m.Name,
                Description = m.Description,
                ThumbnailUrl = m.ThumbnailUrl,
                Author = new CatalogAuthorDto
                {
                    Id = m.AuthorId.ToString(),
                    Username = m.Author.Username,
                    DisplayName = m.Author.Username,
                    AvatarUrl = null
                },
                License = m.License == null ? string.Empty : m.License.ToString()!,
                Categories = m.Categories.Select(c => c.Name).ToList(),
                Tags = m.Tags.Select(t => t.Name).ToList(),
                FilesSize = 0,
                FileCount = 0,
                Downloads = m.Downloads,
                Likes = m.Likes,
                Dislikes = m.Dislikes,
                CreatedAt = m.CreatedAt,
                UpdatedAt = m.UpdatedAt ?? m.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return (totalCount, models);
    }
}
