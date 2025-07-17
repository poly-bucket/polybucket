using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Collections.Domain;
using System;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Collections.AddModelToCollection.Repository
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

        public async Task<bool> ModelExistsAsync(Guid modelId)
        {
            return await _context.Models
                .AsNoTracking()
                .AnyAsync(m => m.Id == modelId);
        }

        public async Task<bool> IsModelInCollectionAsync(Guid collectionId, Guid modelId)
        {
            return await _context.CollectionModels
                .AsNoTracking()
                .AnyAsync(cm => cm.CollectionId == collectionId && cm.ModelId == modelId);
        }

        public async Task AddModelToCollectionAsync(Guid collectionId, Guid modelId)
        {
            var collectionModel = new CollectionModel
            {
                CollectionId = collectionId,
                ModelId = modelId,
                AddedAt = DateTime.UtcNow
            };

            await _context.CollectionModels.AddAsync(collectionModel);
            await _context.SaveChangesAsync();
        }
    }
} 