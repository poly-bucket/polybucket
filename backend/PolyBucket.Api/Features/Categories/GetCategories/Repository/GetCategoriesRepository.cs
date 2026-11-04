using Microsoft.EntityFrameworkCore;
using PolyBucket.Database.Persistence;
using PolyBucket.Api.Features.Categories.GetCategories.Domain;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Categories.GetCategories.Repository
{
    public class GetCategoriesRepository : IGetCategoriesRepository
    {
        private readonly PolyBucketDbContext _context;

        public GetCategoriesRepository(PolyBucketDbContext context)
        {
            _context = context;
        }

        public async Task<GetCategoriesResponse> GetCategoriesAsync(GetCategoriesQuery query)
        {
            var queryable = _context.Categories
                .Where(c => c.DeletedAt == null);

            if (!string.IsNullOrWhiteSpace(query.SearchTerm))
            {
                queryable = queryable.Where(c => c.Name.ToLower().Contains(query.SearchTerm.ToLower()));
            }

            var totalCount = await queryable.CountAsync();

            var rows = await (
                from c in queryable
                join u in _context.Users on c.CreatedById equals u.Id into userJoin
                from creator in userJoin.DefaultIfEmpty()
                orderby c.Name
                select new
                {
                    Category = c,
                    CreatorUsername = creator != null ? creator.Username : null,
                    CreatorEmail = creator != null ? creator.Email : null
                })
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .AsNoTracking()
                .ToListAsync();

            var totalPages = (int)Math.Ceiling((double)totalCount / query.PageSize);
            var hasNextPage = query.Page < totalPages;
            var hasPreviousPage = query.Page > 1;

            var categoryDtos = rows.Select(row =>
            {
                var c = row.Category;
                return new CategoryDto
                {
                    Id = c.Id.ToString(),
                    Name = c.Name,
                    Description = c.Description,
                    Icon = c.Icon,
                    Color = c.Color,
                    CreatedAt = c.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                    CreatedBy = c.CreatedById.ToString(),
                    CreatedByUsername = row.CreatorUsername ?? "Unknown",
                    CreatedByEmail = row.CreatorEmail ?? string.Empty,
                    UpdatedAt = c.UpdatedAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? "Never",
                    UpdatedBy = c.UpdatedById.ToString()
                };
            }).ToList();

            return new GetCategoriesResponse
            {
                Categories = categoryDtos,
                TotalCount = totalCount,
                Page = query.Page,
                PageSize = query.PageSize,
                TotalPages = totalPages,
                HasNextPage = hasNextPage,
                HasPreviousPage = hasPreviousPage
            };
        }
    }
}
