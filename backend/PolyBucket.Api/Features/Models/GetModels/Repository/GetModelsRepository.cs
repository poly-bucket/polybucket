using System;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Data;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Features.ModelModeration.Domain;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Models.GetModels.Repository
{
    public class GetModelsRepository : IGetModelsRepository
    {
        private readonly PolyBucketDbContext _context;

        public GetModelsRepository(PolyBucketDbContext context)
        {
            _context = context;
        }

        public async Task<(IEnumerable<Model> Models, int TotalCount)> GetModelsAsync(int page, int take, string? sortBy)
        {
            var query = _context.Models
                .Include(m => m.Files)
                .Include(m => m.Author)
                .Where(m => m.DeletedAt == null)
                .AsNoTracking()
                .WherePubliclyVisible(_context.ModelModerationRecords);

            var totalCount = await query.CountAsync();
            var models = await ApplyOrdering(query, sortBy)
                .Skip((page - 1) * take)
                .Take(take)
                .ToListAsync();

            return (models, totalCount);
        }

        public static IOrderedQueryable<Model> ApplyOrdering(IQueryable<Model> query, string? sortBy)
        {
            var normalized = sortBy?.Trim().ToLowerInvariant();
            var ordered = normalized switch
            {
                "downloads" => query.OrderByDescending(m => m.Downloads).ThenByDescending(m => m.CreatedAt),
                "likes" => query.OrderByDescending(m => m.Likes).ThenByDescending(m => m.CreatedAt),
                "dislikes" => query.OrderByDescending(m => m.Dislikes).ThenByDescending(m => m.CreatedAt),
                "score" => query.OrderByDescending(m => m.Likes - m.Dislikes).ThenByDescending(m => m.CreatedAt),
                "name" => query.OrderBy(m => m.Name).ThenByDescending(m => m.CreatedAt),
                _ => query.OrderByDescending(m => m.CreatedAt)
            };
            return ordered.ThenBy(m => m.Id);
        }
    }
}
