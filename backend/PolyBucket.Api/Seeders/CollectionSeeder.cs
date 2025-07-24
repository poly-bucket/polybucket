using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Collections.Domain;
using PolyBucket.Api.Features.Collections.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PolyBucket.Api.Data.Seeders
{
    public class CollectionSeeder(PolyBucketDbContext context)
    {
        private readonly PolyBucketDbContext _context = context;

        public async Task SeedAsync()
        {
            if (await _context.Collections.AnyAsync())
            {
                return;
            }

            var admin = await _context.Users.FirstOrDefaultAsync(u => u.Username == "admin");
            if (admin == null)
            {
                return;
            }

            // Get all available models
            var models = await _context.Models.ToListAsync();
            if (!models.Any())
            {
                return;
            }

            var collections = new List<Collection>
            {
                new Collection
                {
                    Id = Guid.NewGuid(),
                    Name = "Gaming & Miniatures",
                    Description = "Collection of miniatures and gaming accessories for tabletop adventures",
                    Visibility = CollectionVisibility.Public,
                    OwnerId = admin.Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-20),
                    UpdatedAt = DateTime.UtcNow.AddDays(-20)
                },
                new Collection
                {
                    Id = Guid.NewGuid(),
                    Name = "Desk & Office",
                    Description = "Practical items to organize and enhance your workspace",
                    Visibility = CollectionVisibility.Public,
                    OwnerId = admin.Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-15),
                    UpdatedAt = DateTime.UtcNow.AddDays(-15)
                },
                new Collection
                {
                    Id = Guid.NewGuid(),
                    Name = "Home Decor",
                    Description = "Beautiful decorative items to spruce up your living space",
                    Visibility = CollectionVisibility.Public,
                    OwnerId = admin.Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-12),
                    UpdatedAt = DateTime.UtcNow.AddDays(-12)
                },
                new Collection
                {
                    Id = Guid.NewGuid(),
                    Name = "Tech Accessories",
                    Description = "Gadgets and accessories for your electronic devices",
                    Visibility = CollectionVisibility.Unlisted,
                    OwnerId = admin.Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-8),
                    UpdatedAt = DateTime.UtcNow.AddDays(-8)
                },
                new Collection
                {
                    Id = Guid.NewGuid(),
                    Name = "Personal Projects",
                    Description = "My personal collection of work-in-progress and experimental designs",
                    Visibility = CollectionVisibility.Private,
                    OwnerId = admin.Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-5),
                    UpdatedAt = DateTime.UtcNow.AddDays(-5)
                },
                new Collection
                {
                    Id = Guid.NewGuid(),
                    Name = "Toys & Fun",
                    Description = "Playful and entertaining models for kids and adults alike",
                    Visibility = CollectionVisibility.Public,
                    OwnerId = admin.Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-2),
                    UpdatedAt = DateTime.UtcNow.AddDays(-2)
                }
            };

            _context.Collections.AddRange(collections);
            await _context.SaveChangesAsync();

            // Now associate models with collections
            var collectionModels = new List<CollectionModel>();

            // Gaming & Miniatures - Dragon, Chess Set
            var gamingCollection = collections[0];
            var dragonModel = models.FirstOrDefault(m => m.Name == "Dragon Figurine");
            var chessModel = models.FirstOrDefault(m => m.Name == "Chess Set");
            
            if (dragonModel != null)
            {
                collectionModels.Add(new CollectionModel
                {
                    CollectionId = gamingCollection.Id,
                    ModelId = dragonModel.Id,
                    AddedAt = DateTime.UtcNow.AddDays(-19)
                });
            }
            
            if (chessModel != null)
            {
                collectionModels.Add(new CollectionModel
                {
                    CollectionId = gamingCollection.Id,
                    ModelId = chessModel.Id,
                    AddedAt = DateTime.UtcNow.AddDays(-18)
                });
            }

            // Desk & Office - Phone Stand, Desk Organizer, Cable Clip
            var officeCollection = collections[1];
            var phoneStandModel = models.FirstOrDefault(m => m.Name == "Phone Stand Pro");
            var organizerModel = models.FirstOrDefault(m => m.Name == "Desk Organizer");
            var cableClipModel = models.FirstOrDefault(m => m.Name == "Cable Clip");

            if (phoneStandModel != null)
            {
                collectionModels.Add(new CollectionModel
                {
                    CollectionId = officeCollection.Id,
                    ModelId = phoneStandModel.Id,
                    AddedAt = DateTime.UtcNow.AddDays(-14)
                });
            }

            if (organizerModel != null)
            {
                collectionModels.Add(new CollectionModel
                {
                    CollectionId = officeCollection.Id,
                    ModelId = organizerModel.Id,
                    AddedAt = DateTime.UtcNow.AddDays(-13)
                });
            }

            if (cableClipModel != null)
            {
                collectionModels.Add(new CollectionModel
                {
                    CollectionId = officeCollection.Id,
                    ModelId = cableClipModel.Id,
                    AddedAt = DateTime.UtcNow.AddDays(-12)
                });
            }

            // Home Decor - Vase, Planter, Lamp Shade, Miniature House
            var decorCollection = collections[2];
            var vaseModel = models.FirstOrDefault(m => m.Name == "Vase Modern");
            var planterModel = models.FirstOrDefault(m => m.Name == "Garden Planter");
            var lampModel = models.FirstOrDefault(m => m.Name == "Lamp Shade");
            var houseModel = models.FirstOrDefault(m => m.Name == "Miniature House");

            foreach (var model in new[] { vaseModel, planterModel, lampModel, houseModel }.Where(m => m != null))
            {
                collectionModels.Add(new CollectionModel
                {
                    CollectionId = decorCollection.Id,
                    ModelId = model!.Id,
                    AddedAt = DateTime.UtcNow.AddDays(-11)
                });
            }

            // Tech Accessories - Phone Stand, Cable Clip (shared with office)
            var techCollection = collections[3];
            if (phoneStandModel != null)
            {
                collectionModels.Add(new CollectionModel
                {
                    CollectionId = techCollection.Id,
                    ModelId = phoneStandModel.Id,
                    AddedAt = DateTime.UtcNow.AddDays(-7)
                });
            }

            if (cableClipModel != null)
            {
                collectionModels.Add(new CollectionModel
                {
                    CollectionId = techCollection.Id,
                    ModelId = cableClipModel.Id,
                    AddedAt = DateTime.UtcNow.AddDays(-6)
                });
            }

            // Personal Projects - Sample Model
            var personalCollection = collections[4];
            var sampleModel = models.FirstOrDefault(m => m.Name == "Sample Model");
            if (sampleModel != null)
            {
                collectionModels.Add(new CollectionModel
                {
                    CollectionId = personalCollection.Id,
                    ModelId = sampleModel.Id,
                    AddedAt = DateTime.UtcNow.AddDays(-4)
                });
            }

            // Toys & Fun - Robot, Keychain Heart
            var toysCollection = collections[5];
            var robotModel = models.FirstOrDefault(m => m.Name == "Robot Toy");
            var keychainModel = models.FirstOrDefault(m => m.Name == "Keychain Heart");

            if (robotModel != null)
            {
                collectionModels.Add(new CollectionModel
                {
                    CollectionId = toysCollection.Id,
                    ModelId = robotModel.Id,
                    AddedAt = DateTime.UtcNow.AddDays(-1)
                });
            }

            if (keychainModel != null)
            {
                collectionModels.Add(new CollectionModel
                {
                    CollectionId = toysCollection.Id,
                    ModelId = keychainModel.Id,
                    AddedAt = DateTime.UtcNow
                });
            }

            _context.CollectionModels.AddRange(collectionModels);
            await _context.SaveChangesAsync();
        }
    }
} 