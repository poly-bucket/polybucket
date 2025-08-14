using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Models.Domain;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Categories.DeleteCategory.Repository
{
    public class DeleteCategoryRepository : IDeleteCategoryRepository
    {
        private readonly PolyBucketDbContext _context;

        public DeleteCategoryRepository(PolyBucketDbContext context)
        {
            _context = context;
        }

        public async Task<Category?> GetCategoryByIdAsync(Guid id)
        {
            return await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id && c.DeletedAt == null);
        }

        public async Task<bool> IsCategoryInUseAsync(Guid id)
        {
            return await _context.Models
                .AnyAsync(m => m.Categories.Any(c => c.Id == id) && m.DeletedAt == null);
        }

        public async Task<Category> DeleteCategoryAsync(Guid id, Guid deletedById)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                throw new InvalidOperationException($"Category with ID '{id}' not found");
            }

            category.DeletedAt = DateTime.UtcNow;
            category.DeletedById = deletedById;
            category.UpdatedAt = DateTime.UtcNow;
            category.UpdatedById = deletedById;

            await _context.SaveChangesAsync();
            return category;
        }
    }
}
