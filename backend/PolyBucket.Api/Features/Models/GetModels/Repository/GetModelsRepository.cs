using System;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Models.Shared.Domain;
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
                .Where(m => m.IsPublic)
                .AsNoTracking();

            if (string.Equals(sortBy, "createdAt", StringComparison.OrdinalIgnoreCase))
            {
                query = query.OrderByDescending(m => m.CreatedAt);
            }

            var totalCount = await query.CountAsync();
            var models = await query
                .Skip((page - 1) * take)
                .Take(take)
                .ToListAsync();

            return (models, totalCount);
        }
    }
}
