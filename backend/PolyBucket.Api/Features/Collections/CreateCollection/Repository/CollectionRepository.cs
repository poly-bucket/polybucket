using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Collections.Domain;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Collections.CreateCollection.Repository
{
    public class CollectionRepository : ICollectionRepository
    {
        private readonly PolyBucketDbContext _context;

        public CollectionRepository(PolyBucketDbContext context)
        {
            _context = context;
        }

        public async Task<Collection> CreateCollectionAsync(Collection collection)
        {
            await _context.Collections.AddAsync(collection);
            await _context.SaveChangesAsync();
            return collection;
        }
    }
} 