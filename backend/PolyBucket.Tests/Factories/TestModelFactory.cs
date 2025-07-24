using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Models.Domain;
using PolyBucket.Api.Features.Models.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace PolyBucket.Tests.Factories
{
    public class TestModelFactory(PolyBucketDbContext context)
    {
        private readonly PolyBucketDbContext _context = context;

        public async Task<IEnumerable<Model>> CreateTestModels(int count, Guid? userId = null)
        {
            var models = new List<Model>();

            // Ensure we have a valid user ID
            if (!userId.HasValue)
            {
                var user = await _context.Users.FirstOrDefaultAsync();
                if (user == null)
                {
                    throw new InvalidOperationException("No users found in database. Please create a user first.");
                }
                userId = user.Id;
            }

            for (int i = 0; i < count; i++)
            {
                var model = new Model
                {
                    Id = Guid.NewGuid(),
                    Name = $"Test Model {i + 1}",
                    Description = $"Test Description for Model {i + 1}",
                    License = LicenseTypes.MIT,
                    Privacy = PrivacySettings.Public,
                    Categories = new List<Category>(),
                    AIGenerated = false,
                    WIP = false,
                    NSFW = false,
                    IsRemix = false,
                    AuthorId = userId.Value,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                };

                _context.Models.Add(model);
                models.Add(model);
            }

            await _context.SaveChangesAsync();
            return models;
        }

        public async Task<Model> CreateTestModel(string name = "Test Model", string description = "Test Description", Guid? userId = null)
        {
            // Ensure we have a valid user ID
            if (!userId.HasValue)
            {
                var user = await _context.Users.FirstOrDefaultAsync();
                if (user == null)
                {
                    throw new InvalidOperationException("No users found in database. Please create a user first.");
                }
                userId = user.Id;
            }

            var model = new Model
            {
                Id = Guid.NewGuid(),
                Name = name,
                Description = description,
                License = LicenseTypes.MIT,
                Privacy = PrivacySettings.Public,
                Categories = new List<Category>(),
                AIGenerated = false,
                WIP = false,
                NSFW = false,
                IsRemix = false,
                AuthorId = userId.Value,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Models.Add(model);
            await _context.SaveChangesAsync();
            return model;
        }

        public async Task<Model> CreateTestModel(Guid userId)
        {
            return await CreateTestModel("Test Model", "Test Description", userId);
        }

        public async Task<ModelVersion> CreateTestModelVersion(Guid modelId, string name = "Test Version", string notes = "Test Version Notes")
        {
            // Verify the model exists
            var model = await _context.Models.FindAsync(modelId);
            if (model == null)
            {
                throw new InvalidOperationException($"Model with ID {modelId} not found.");
            }

            var modelVersion = new ModelVersion
            {
                Id = Guid.NewGuid(),
                ModelId = modelId,
                Name = name,
                Notes = notes,
                VersionNumber = 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.ModelVersions.Add(modelVersion);
            await _context.SaveChangesAsync();
            return modelVersion;
        }
    }
} 