using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Collections.Domain;
using System;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Collections.DeleteCollection.Repository
{
    public class CollectionRepository(PolyBucketDbContext context) : ICollectionRepository
    {
        private readonly PolyBucketDbContext _context = context;

        public async Task<Collection?> GetCollectionByIdAsync(Guid id)
        {
            return await _context.Collections.FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task DeleteCollectionAsync(Collection collection)
        {
            _context.Collections.Remove(collection);
            await _context.SaveChangesAsync();
        }
    }
} 