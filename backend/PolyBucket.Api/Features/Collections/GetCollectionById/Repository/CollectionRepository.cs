using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Collections.Domain;
using System;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Collections.GetCollectionById.Repository
{
    public class CollectionRepository(PolyBucketDbContext context) : ICollectionRepository
    {
        private readonly PolyBucketDbContext _context = context;

        public async Task<Collection?> GetCollectionByIdAsync(Guid id)
        {
            return await _context.Collections
                .Include(c => c.CollectionModels)
                .ThenInclude(cm => cm.Model)
                .Include(c => c.Owner)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);
        }
    }
} 