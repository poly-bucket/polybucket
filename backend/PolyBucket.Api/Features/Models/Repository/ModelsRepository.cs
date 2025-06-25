using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PolyBucket.Api.Features.Models.Domain;

namespace PolyBucket.Api.Features.Models.Repository
{
    public class ModelsRepository : IModelsRepository
    {
        private readonly PolyBucketDbContext _context;

        public ModelsRepository(PolyBucketDbContext context)
        {
            _context = context;
        }

        public async Task<(IEnumerable<Model> Models, int TotalCount)> GetModelsAsync(int page, int take)
        {
            var query = _context.Models
                // .Include(m => m.Files)
                .AsNoTracking();

            var totalCount = await query.CountAsync();
            var models = await query
                .Skip((page - 1) * take)
                .Take(take)
                .ToListAsync();

            return (models, totalCount);
        }

        public async Task<Model> GetModelByIdAsync(Guid id)
        {
            return await _context.Models
                // .Include(m => m.Files)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);
        }
    }
} 