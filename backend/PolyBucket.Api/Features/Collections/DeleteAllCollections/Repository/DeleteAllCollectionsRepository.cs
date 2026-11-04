using Microsoft.EntityFrameworkCore;
using PolyBucket.Database.Entities.Comments;
using PolyBucket.Database.Persistence;

namespace PolyBucket.Api.Features.Collections.DeleteAllCollections.Repository
{
    public class DeleteAllCollectionsRepository(PolyBucketDbContext context) : IDeleteAllCollectionsRepository
    {
        private readonly PolyBucketDbContext _context = context;

        public async Task<int> DeleteAllCollectionsAndReturnCountAsync(CancellationToken cancellationToken = default)
        {
            var collections = await _context.Collections.ToListAsync(cancellationToken);
            if (collections.Count == 0)
            {
                return 0;
            }

            var collectionComments = await _context.EnhancedComments
                .Where(c => c.TargetType == CommentTargetType.Collection)
                .ToListAsync(cancellationToken);
            _context.EnhancedComments.RemoveRange(collectionComments);

            var collectionModels = await _context.CollectionModels.ToListAsync(cancellationToken);
            _context.CollectionModels.RemoveRange(collectionModels);

            _context.Collections.RemoveRange(collections);
            await _context.SaveChangesAsync(cancellationToken);
            return collections.Count;
        }
    }
}
