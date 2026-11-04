using Microsoft.EntityFrameworkCore;
using PolyBucket.Database.Persistence;

namespace PolyBucket.Api.Features.Categories.DeleteAllCategories.Repository
{
    public class DeleteAllCategoriesRepository(PolyBucketDbContext context) : IDeleteAllCategoriesRepository
    {
        private readonly PolyBucketDbContext _context = context;

        public async Task<int> DeleteAllCategoriesAndReturnCountAsync(
            Guid deletedById,
            CancellationToken cancellationToken = default)
        {
            var models = await _context.Models
                .Include(m => m.Categories)
                .ToListAsync(cancellationToken);

            foreach (var model in models)
            {
                model.Categories.Clear();
            }

            var categories = await _context.Categories
                .Where(c => c.DeletedAt == null)
                .ToListAsync(cancellationToken);

            if (categories.Count == 0)
            {
                if (models.Count > 0)
                {
                    await _context.SaveChangesAsync(cancellationToken);
                }

                return 0;
            }

            var now = DateTime.UtcNow;
            foreach (var category in categories)
            {
                category.DeletedAt = now;
                category.DeletedById = deletedById;
                category.UpdatedAt = now;
                category.UpdatedById = deletedById;
            }

            await _context.SaveChangesAsync(cancellationToken);
            return categories.Count;
        }
    }
}
