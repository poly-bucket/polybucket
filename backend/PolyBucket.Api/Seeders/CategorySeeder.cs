using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Models.Shared.Domain;
using PolyBucket.Api.Features.Models.AddCategoryToModel.Domain;
using PolyBucket.Api.Features.Models.Shared.Domain.Enums;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PolyBucket.Api.Seeders
{
    public class CategorySeeder
    {
        private readonly PolyBucketDbContext _context;

        public CategorySeeder(PolyBucketDbContext context)
        {
            _context = context;
        }

        public async Task SeedAsync()
        {
            if (await _context.Categories.AnyAsync())
            {
                return; // Categories already seeded
            }

            var adminUser = await _context.Users
                .Where(u => u.Role.Name == "Admin")
                .FirstOrDefaultAsync();

            if (adminUser == null)
            {
                throw new InvalidOperationException("Admin user not found. Please ensure admin user is created before seeding categories.");
            }

            var categories = Enum.GetValues<ModelCategories>()
                .Select(category => new Category
                {
                    Id = Guid.NewGuid(),
                    Name = category.ToString(),
                    CreatedById = adminUser.Id,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedById = adminUser.Id,
                    UpdatedAt = DateTime.UtcNow
                })
                .ToList();

            await _context.Categories.AddRangeAsync(categories);
            await _context.SaveChangesAsync();
        }
    }
}
