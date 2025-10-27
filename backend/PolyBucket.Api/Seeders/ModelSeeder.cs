using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Models.Shared.Domain;
using PolyBucket.Api.Features.Models.CreateModel.Domain;
using PolyBucket.Api.Features.Models.Shared.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PolyBucket.Api.Data.Seeders
{
    public class ModelSeeder(PolyBucketDbContext context)
    {
        private readonly PolyBucketDbContext _context = context;

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

            var models = new List<Model>
            {
                new Model
                {
                    Id = Guid.NewGuid(),
                    Name = "Sample Model",
                    Description = "A sample 3D model for testing",
                    AuthorId = admin.Id,
                    License = LicenseTypes.MIT,
                    Privacy = PrivacySettings.Public,
                    IsPublic = true,
                    AIGenerated = false,
                    WIP = false,
                    NSFW = false,
                    IsRemix = false,
                    Downloads = 245,
                    Likes = 18,
                    ThumbnailUrl = null, // No thumbnail to test MiniModelViewer
                    CreatedAt = DateTime.UtcNow.AddDays(-30),
                    UpdatedAt = DateTime.UtcNow.AddDays(-30),
                },
                new Model
                {
                    Id = Guid.NewGuid(),
                    Name = "Dragon Figurine",
                    Description = "Detailed dragon miniature perfect for tabletop gaming",
                    AuthorId = admin.Id,
                    License = LicenseTypes.CCBy4,
                    Privacy = PrivacySettings.Public,
                    IsPublic = true,
                    IsFeatured = true,
                    AIGenerated = false,
                    WIP = false,
                    NSFW = false,
                    IsRemix = false,
                    Downloads = 892,
                    Likes = 76,
                    ThumbnailUrl = "https://via.placeholder.com/300x200/dc2626/ffffff?text=Dragon",
                    CreatedAt = DateTime.UtcNow.AddDays(-25),
                    UpdatedAt = DateTime.UtcNow.AddDays(-25),
                },
                new Model
                {
                    Id = Guid.NewGuid(),
                    Name = "Phone Stand Pro",
                    Description = "Adjustable phone stand with cable management",
                    AuthorId = admin.Id,
                    License = LicenseTypes.MIT,
                    Privacy = PrivacySettings.Public,
                    IsPublic = true,
                    AIGenerated = false,
                    WIP = false,
                    NSFW = false,
                    IsRemix = false,
                    Downloads = 1247,
                    Likes = 102,
                    ThumbnailUrl = "https://via.placeholder.com/300x200/059669/ffffff?text=Phone+Stand",
                    CreatedAt = DateTime.UtcNow.AddDays(-20),
                    UpdatedAt = DateTime.UtcNow.AddDays(-20),
                },
                new Model
                {
                    Id = Guid.NewGuid(),
                    Name = "Miniature House",
                    Description = "Charming little house model for dioramas",
                    AuthorId = admin.Id,
                    License = LicenseTypes.CCBy4,
                    Privacy = PrivacySettings.Public,
                    IsPublic = true,
                    AIGenerated = false,
                    WIP = false,
                    NSFW = false,
                    IsRemix = false,
                    Downloads = 567,
                    Likes = 43,
                    ThumbnailUrl = "https://via.placeholder.com/300x200/ea580c/ffffff?text=Mini+House",
                    CreatedAt = DateTime.UtcNow.AddDays(-18),
                    UpdatedAt = DateTime.UtcNow.AddDays(-18),
                },
                new Model
                {
                    Id = Guid.NewGuid(),
                    Name = "Desk Organizer",
                    Description = "Multi-compartment desk organizer for office supplies",
                    AuthorId = admin.Id,
                    License = LicenseTypes.MIT,
                    Privacy = PrivacySettings.Public,
                    IsPublic = true,
                    AIGenerated = false,
                    WIP = false,
                    NSFW = false,
                    IsRemix = false,
                    Downloads = 789,
                    Likes = 64,
                    ThumbnailUrl = "https://via.placeholder.com/300x200/7c3aed/ffffff?text=Organizer",
                    CreatedAt = DateTime.UtcNow.AddDays(-15),
                    UpdatedAt = DateTime.UtcNow.AddDays(-15),
                },
                new Model
                {
                    Id = Guid.NewGuid(),
                    Name = "Robot Toy",
                    Description = "Articulated robot figure with moveable parts",
                    AuthorId = admin.Id,
                    License = LicenseTypes.MIT,
                    Privacy = PrivacySettings.Public,
                    IsPublic = true,
                    AIGenerated = false,
                    WIP = false,
                    NSFW = false,
                    IsRemix = false,
                    Downloads = 423,
                    Likes = 31,
                    ThumbnailUrl = "https://via.placeholder.com/300x200/0891b2/ffffff?text=Robot",
                    CreatedAt = DateTime.UtcNow.AddDays(-12),
                    UpdatedAt = DateTime.UtcNow.AddDays(-12),
                },
                new Model
                {
                    Id = Guid.NewGuid(),
                    Name = "Garden Planter",
                    Description = "Decorative planter for small plants and succulents",
                    AuthorId = admin.Id,
                    License = LicenseTypes.CCBy4,
                    Privacy = PrivacySettings.Public,
                    IsPublic = true,
                    AIGenerated = false,
                    WIP = false,
                    NSFW = false,
                    IsRemix = false,
                    Downloads = 334,
                    Likes = 28,
                    ThumbnailUrl = "https://via.placeholder.com/300x200/16a34a/ffffff?text=Planter",
                    CreatedAt = DateTime.UtcNow.AddDays(-10),
                    UpdatedAt = DateTime.UtcNow.AddDays(-10),
                },
                new Model
                {
                    Id = Guid.NewGuid(),
                    Name = "Chess Set",
                    Description = "Complete chess set with detailed pieces",
                    AuthorId = admin.Id,
                    License = LicenseTypes.MIT,
                    Privacy = PrivacySettings.Public,
                    IsPublic = true,
                    IsFeatured = true,
                    AIGenerated = false,
                    WIP = false,
                    NSFW = false,
                    IsRemix = false,
                    Downloads = 1876,
                    Likes = 145,
                    ThumbnailUrl = "https://via.placeholder.com/300x200/1f2937/ffffff?text=Chess+Set",
                    CreatedAt = DateTime.UtcNow.AddDays(-8),
                    UpdatedAt = DateTime.UtcNow.AddDays(-8),
                },
                new Model
                {
                    Id = Guid.NewGuid(),
                    Name = "Cable Clip",
                    Description = "Simple cable management clip for desks",
                    AuthorId = admin.Id,
                    License = LicenseTypes.MIT,
                    Privacy = PrivacySettings.Public,
                    IsPublic = true,
                    AIGenerated = false,
                    WIP = false,
                    NSFW = false,
                    IsRemix = false,
                    Downloads = 2104,
                    Likes = 167,
                    ThumbnailUrl = "https://via.placeholder.com/300x200/6b7280/ffffff?text=Cable+Clip",
                    CreatedAt = DateTime.UtcNow.AddDays(-6),
                    UpdatedAt = DateTime.UtcNow.AddDays(-6),
                },
                new Model
                {
                    Id = Guid.NewGuid(),
                    Name = "Vase Modern",
                    Description = "Contemporary vase design with geometric patterns",
                    AuthorId = admin.Id,
                    License = LicenseTypes.CCBy4,
                    Privacy = PrivacySettings.Public,
                    IsPublic = true,
                    AIGenerated = false,
                    WIP = false,
                    NSFW = false,
                    IsRemix = false,
                    Downloads = 445,
                    Likes = 39,
                    ThumbnailUrl = "https://via.placeholder.com/300x200/ec4899/ffffff?text=Modern+Vase",
                    CreatedAt = DateTime.UtcNow.AddDays(-4),
                    UpdatedAt = DateTime.UtcNow.AddDays(-4),
                },
                new Model
                {
                    Id = Guid.NewGuid(),
                    Name = "Keychain Heart",
                    Description = "Cute heart-shaped keychain",
                    AuthorId = admin.Id,
                    License = LicenseTypes.MIT,
                    Privacy = PrivacySettings.Public,
                    IsPublic = true,
                    AIGenerated = false,
                    WIP = false,
                    NSFW = false,
                    IsRemix = false,
                    Downloads = 678,
                    Likes = 52,
                    ThumbnailUrl = "https://via.placeholder.com/300x200/f59e0b/ffffff?text=Heart+Key",
                    CreatedAt = DateTime.UtcNow.AddDays(-2),
                    UpdatedAt = DateTime.UtcNow.AddDays(-2),
                },
                new Model
                {
                    Id = Guid.NewGuid(),
                    Name = "Lamp Shade",
                    Description = "Artistic lamp shade with intricate cutout patterns",
                    AuthorId = admin.Id,
                    License = LicenseTypes.CCBy4,
                    Privacy = PrivacySettings.Public,
                    IsPublic = true,
                    AIGenerated = false,
                    WIP = false,
                    NSFW = false,
                    IsRemix = false,
                    Downloads = 234,
                    Likes = 19,
                    ThumbnailUrl = "https://via.placeholder.com/300x200/8b5cf6/ffffff?text=Lamp+Shade",
                    CreatedAt = DateTime.UtcNow.AddDays(-1),
                    UpdatedAt = DateTime.UtcNow.AddDays(-1),
                }
            };

            _context.Models.AddRange(models);
            await _context.SaveChangesAsync();

            // Add a test file to the first model to test MiniModelViewer
            var firstModel = models[0];
            var testFile = new ModelFile
            {
                Id = Guid.NewGuid(),
                Model = firstModel,
                Name = "test-model.stl",
                Path = "test/path/test-model.stl",
                Size = 1024 * 1024, // 1MB
                MimeType = "application/sla",
                CreatedAt = DateTime.UtcNow,
                CreatedById = admin.Id,
                UpdatedAt = DateTime.UtcNow,
                UpdatedById = admin.Id
            };

            _context.ModelFiles.Add(testFile);
            await _context.SaveChangesAsync();
        }
    }
} 