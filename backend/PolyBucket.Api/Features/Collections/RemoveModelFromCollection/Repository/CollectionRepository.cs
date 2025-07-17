using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Collections.Domain;
using System;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Collections.RemoveModelFromCollection.Repository
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
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<bool> IsModelInCollectionAsync(Guid collectionId, Guid modelId)
        {
            return await _context.CollectionModels
                .AsNoTracking()
                .AnyAsync(cm => cm.CollectionId == collectionId && cm.ModelId == modelId);
        }

        public async Task RemoveModelFromCollectionAsync(Guid collectionId, Guid modelId)
        {
            var collectionModel = await _context.CollectionModels
                .FirstOrDefaultAsync(cm => cm.CollectionId == collectionId && cm.ModelId == modelId);

            if (collectionModel != null)
            {
                _context.CollectionModels.Remove(collectionModel);
                await _context.SaveChangesAsync();
            }
        }
    }
} 