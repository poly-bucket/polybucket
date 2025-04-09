using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Models.Domain;
using PolyBucket.Api.Features.Models.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PolyBucket.Api.Data.Seeders
{
    public class ModelSeeder
    {
        private readonly PolyBucketDbContext _context;

        public ModelSeeder(PolyBucketDbContext context)
        {
            _context = context;
        }

        public async Task SeedAsync()
        {
            if (await _context.Models.AnyAsync(m => m.Name == "Sample Model"))
            {
                return;
            }

            var admin = await _context.Users.FirstOrDefaultAsync(u => u.Username == "admin");
            if (admin == null)
            {
                // Admin user is required for seeding models
                return;
            }

            var model = new Model
            {
                Id = Guid.NewGuid(),
                Name = "Sample Model",
                Description = "A sample 3D model for testing",
                Author = admin,
                License = LicenseTypes.MIT,
                Privacy = PrivacySettings.Public,
                Categories = new List<Category>(), // Empty for now, can be populated later
                AIGenerated = false,
                WIP = false,
                NSFW = false,
                IsRemix = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };

            _context.Models.Add(model);
            await _context.SaveChangesAsync();
        }
    }
} 