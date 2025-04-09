using Core.Enumerations;
using Core.Models.Models;
using Core.Models.Users;
using Microsoft.EntityFrameworkCore;
using File = Core.Models.Files.File;
using FileExtension = Core.Models.Files.FileExtension;

namespace Database.Seeders;

public class ModelSeeder
{
    private readonly Context _context;

    public ModelSeeder(Context context)
    {
        _context = context;
    }

    public async Task SeedAsync()
    {
        var sampleModelId = Guid.NewGuid();
        if (await _context.Models.FindAsync(sampleModelId) != null)
            return;

        var admin = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == "admin");
        if (admin == null)
            return;

        var model = new Model
        {
            Id = sampleModelId,
            Name = "Sample Model",
            Description = "A sample 3D model for testing",
            Author = admin,
            License = LicenseTypes.MIT,
            Privacy = PrivacySettings.Public,
            Categories = new List<ModelCategories> { ModelCategories.Art, ModelCategories.Technology },
            AIGenerated = false,
            WIP = false,
            NSFW = false,
            IsRemix = "false",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Files = new List<File>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "sample.stl",
                    Path = "/files/sample.stl",
                    Size = 1024,
                    MimeType = "model/stl",
                    Extension = FileExtension.stl,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            }
        };

        _context.Models.Add(model);
        await _context.SaveChangesAsync();
    }
}