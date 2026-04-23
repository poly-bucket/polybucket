using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Data;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Features.Models.AddCategoryToModel.Domain;
using System;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Categories.UpdateCategory.Repository
{
    public class UpdateCategoryRepository : IUpdateCategoryRepository
    {
        private readonly PolyBucketDbContext _context;

        public UpdateCategoryRepository(PolyBucketDbContext context)
        {
            _context = context;
        }

        public async Task<Category?> GetCategoryByIdAsync(Guid id)
        {
            return await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id && c.DeletedAt == null);
        }

        public async Task<Category?> GetCategoryByNameAsync(string name)
        {
            return await _context.Categories
                .FirstOrDefaultAsync(c => c.Name.ToLower() == name.ToLower() && c.DeletedAt == null);
        }

        public async Task<Category> UpdateCategoryAsync(Guid id, string name, Guid updatedById)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                throw new InvalidOperationException($"Category with ID '{id}' not found");
            }

            category.Name = name.Trim();
            category.UpdatedAt = DateTime.UtcNow;
            category.UpdatedById = updatedById;

            await _context.SaveChangesAsync();
            return category;
        }
    }
}
