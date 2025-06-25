using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Models.Domain;
using PolyBucket.Api.Features.Models.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PolyBucket.Tests.Factories
{
    public class TestModelFactory
    {
        private readonly PolyBucketDbContext _context;

        public TestModelFactory(PolyBucketDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Model>> CreateTestModels(int count)
        {
            var models = new List<Model>();

            for (int i = 0; i < count; i++)
            {
                var model = new Model
                {
                    Name = $"Test Model {i + 1}",
                    Description = $"Test Description for Model {i + 1}",
                    License = LicenseTypes.MIT,
                    Privacy = PrivacySettings.Public,
                    Categories = new List<ModelCategories> { ModelCategories.Other },
                    AIGenerated = false,
                    WIP = false,
                    NSFW = false,
                    IsRemix = "",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                };

                _context.Models.Add(model);
                models.Add(model);
            }

            await _context.SaveChangesAsync();
            return models;
        }
    }
} 