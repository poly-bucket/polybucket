using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Collections.Domain;
using System;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Collections.FavoriteCollection.Repository
{
    public class CollectionRepository : ICollectionRepository
    {
        private readonly PolyBucketDbContext _context;

        public CollectionRepository(PolyBucketDbContext context)
        {
            _context = context;
        }

        public async Task<Collection?> GetCollectionByIdAsync(Guid id)
        {
            return await _context.Collections
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Collection> UpdateCollectionAsync(Collection collection)
        {
            _context.Collections.Update(collection);
            await _context.SaveChangesAsync();
            return collection;
        }
    }
}
