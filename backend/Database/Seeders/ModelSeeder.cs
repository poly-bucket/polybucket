using Core.Models.Models;
using Core.Models.Users;
using Core.Models.Files;
using Core.Enumerations;
using System.Text.Json;

namespace Database.Seeders
{
    public class ModelSeeder
    {
        private readonly Context _context;
        private readonly string _buildPath;
        private readonly string _filesPath;

        public ModelSeeder(Context context)
        {
            _context = context;
            _buildPath = Path.GetDirectoryName(typeof(ModelSeeder).Assembly.Location) ?? "";
            _filesPath = Path.Combine(_buildPath, "files");
        }

        public void Seed()
        {
            if (_context.Models.Any())
            {
                return;
            }

            // Ensure files directory exists
            Directory.CreateDirectory(_filesPath);

            // Get or create admin user for ownership
            var admin = _context.Users.FirstOrDefault(u => u.Email == "admin@localhost")
                ?? throw new Exception("Admin user not found. Please run AdminSeeder first.");

            var models = new List<Model>
            {
                new Model
                {
                    Name = "Basic Sphere",
                    Description = "A simple sphere model for testing",
                    License = LicenseTypes.MIT,
                    Privacy = PrivacySettings.Public,
                    Categories = new List<ModelCategories> { ModelCategories.Adventure },
                    AIGenerated = false,
                    WIP = false,
                    NSFW = false,
                    IsRemix = "",
                    Author = admin,
                    CreatedById = admin.Id,
                    UpdatedById = admin.Id,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Files = new List<Core.Models.Files.File>
                    {
                        CreateFile("sphere.stl", admin)
                    }
                },
                new Model
                {
                    Name = "Basic Cube",
                    Description = "A simple cube model for testing",
                    License = LicenseTypes.MIT,
                    Privacy = PrivacySettings.Public,
                    Categories = new List<ModelCategories> { ModelCategories.Accessories, ModelCategories.Gadget },
                    AIGenerated = false,
                    WIP = false,
                    NSFW = false,
                    IsRemix = "",
                    Author = admin,
                    CreatedById = admin.Id,
                    UpdatedById = admin.Id,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Files = new List<Core.Models.Files.File>
                    {
                        CreateFile("cube.stl", admin)
                    }
                }
                // We can add more models here once you show me the available .stl files
            };

            _context.Models.AddRange(models);
            _context.SaveChanges();

            // Copy files to destination
            foreach (var model in models)
            {
                foreach (var file in model.Files)
                {
                    var sourcePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Seeders", "Files", file.Name);
                    var destPath = Path.Combine(_filesPath, file.Path);

                    if (System.IO.File.Exists(sourcePath))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(destPath)!);
                        System.IO.File.Copy(sourcePath, destPath, true);
                    }
                }
            }
        }

        private Core.Models.Files.File CreateFile(string filename, User owner)
        {
            var fileInfo = new FileInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Seeders", "Files", filename));

            if (!fileInfo.Exists)
            {
                throw new FileNotFoundException($"Seed file not found: {filename} | {fileInfo.Directory}");
            }

            var relativePath = Path.Combine(
                Guid.NewGuid().ToString(),
                filename
            );

            return new Core.Models.Files.File
            {
                Name = filename,
                Path = relativePath,
                Extension = FileExtension.stl,
                MimeType = "model/stl",
                Size = fileInfo.Length,
                CreatedById = owner.Id,
                UpdatedById = owner.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }
    }
}