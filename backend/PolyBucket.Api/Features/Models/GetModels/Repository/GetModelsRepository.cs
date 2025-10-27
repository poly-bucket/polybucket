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

        public async Task<(IEnumerable<Model> Models, int TotalCount)> GetModelsAsync(int page, int take)
        {
            var query = _context.Models
                .Include(m => m.Files)
                .Include(m => m.Author)
                .Where(m => m.DeletedAt == null) // Exclude soft-deleted models
                .AsNoTracking();

            var totalCount = await query.CountAsync();
            var models = await query
                .Skip((page - 1) * take)
                .Take(take)
                .ToListAsync();

            return (models, totalCount);
        }
    }
}
