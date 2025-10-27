using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Models.Shared.Domain;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Models.GetModelById.Repository
{
    public class GetModelByIdRepository : IGetModelByIdRepository
    {
        private readonly PolyBucketDbContext _context;

        public GetModelByIdRepository(PolyBucketDbContext context)
        {
            _context = context;
        }

        public async Task<Model?> GetModelByIdAsync(Guid id)
        {
            return await _context.Models
                .Include(m => m.Files)
                .Include(m => m.Author)
                .Include(m => m.Categories)
                .Include(m => m.Tags)
                .Include(m => m.Versions)
                .Include(m => m.Comments)
                .Include(m => m.LikeCollection)
                .Where(m => m.DeletedAt == null) // Exclude soft-deleted models
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);
        }
    }
}
