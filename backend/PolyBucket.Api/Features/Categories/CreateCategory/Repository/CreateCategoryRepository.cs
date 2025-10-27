using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Models.Shared.Domain;
using PolyBucket.Api.Features.Models.AddCategoryToModel.Domain;
using PolyBucket.Api.Features.Models.AddCategoryToModel.Domain;
using System;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Categories.CreateCategory.Repository
{
    public class CreateCategoryRepository : ICreateCategoryRepository
    {
        private readonly PolyBucketDbContext _context;

        public CreateCategoryRepository(PolyBucketDbContext context)
        {
            _context = context;
        }

        public async Task<Category> CreateCategoryAsync(Category category)
        {
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task<Category?> GetCategoryByIdAsync(Guid id)
        {
            return await _context.Categories
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id && c.DeletedAt == null);
        }

        public async Task<Category?> GetCategoryByNameAsync(string name)
        {
            return await _context.Categories
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Name.ToLower() == name.ToLower() && c.DeletedAt == null);
        }
    }
}
