using Core.Models.Models;
using Core.Enumerations;
using Database;
using Core.Models.Files;

namespace Tests.Factories;

public class TestModelFactory
{
    private readonly Context _context;

    public TestModelFactory(Context context)
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
                Files = new List<Core.Models.Files.File>
                {
                    new Core.Models.Files.File
                    {
                        Name = $"test_file_{i + 1}.stl",
                        Path = $"/test/path/test_file_{i + 1}.stl",
                        Size = 1024,
                        MimeType = "model/stl",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    }
                }
            };

            _context.Models.Add(model);
            models.Add(model);
        }

        await _context.SaveChangesAsync();
        return models;
    }
} 