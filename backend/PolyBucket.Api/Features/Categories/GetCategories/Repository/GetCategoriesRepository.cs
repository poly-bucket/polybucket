using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Data;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Features.Models.AddCategoryToModel.Domain;
using PolyBucket.Api.Features.Categories.GetCategories.Domain;
using System.Collections.Generic;
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

            // Apply search filter if provided
            if (!string.IsNullOrWhiteSpace(query.SearchTerm))
            {
                queryable = queryable.Where(c => c.Name.ToLower().Contains(query.SearchTerm.ToLower()));
            }

            // Get total count for pagination
            var totalCount = await queryable.CountAsync();

            // Apply pagination
            var categories = await queryable
                .OrderBy(c => c.Name)
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .AsNoTracking()
                .ToListAsync();

            // Calculate pagination metadata
            var totalPages = (int)Math.Ceiling((double)totalCount / query.PageSize);
            var hasNextPage = query.Page < totalPages;
            var hasPreviousPage = query.Page > 1;

            // Convert to DTOs
            var categoryDtos = categories.Select(c => new CategoryDto
            {
                Id = c.Id.ToString(),
                Name = c.Name,
                CreatedAt = c.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                CreatedBy = c.CreatedById.ToString(),
                UpdatedAt = c.UpdatedAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? "Never",
                UpdatedBy = c.UpdatedById.ToString()
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
