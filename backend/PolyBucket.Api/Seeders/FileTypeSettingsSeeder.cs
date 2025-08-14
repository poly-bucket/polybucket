using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.SystemSettings.Domain;
using Microsoft.Extensions.Logging;

namespace PolyBucket.Api.Seeders
{
    public class FileTypeSettingsSeeder(PolyBucketDbContext context, ILogger<FileTypeSettingsSeeder> logger)
    {
        private readonly PolyBucketDbContext _context = context;
        private readonly ILogger<FileTypeSettingsSeeder> _logger = logger;

        public async Task SeedAsync()
        {
            if (await _context.FileTypeSettings.AnyAsync())
            {
                _logger.LogInformation("File type settings already exist, skipping seeding");
                return;
            }

            var adminUser = await _context.Users
                .Where(u => u.Role.Name == "Admin")
                .FirstOrDefaultAsync();

            if (adminUser == null)
            {
                throw new InvalidOperationException("Admin user not found. Please ensure admin user is created before seeding file type settings.");
            }

            var fileTypes = new List<FileTypeSettings>
            {
                // 3D Model Formats
                new FileTypeSettings
                {
                    Id = Guid.NewGuid(),
                    FileExtension = ".stl",
                    Enabled = true,
                    MaxFileSizeBytes = 500L * 1024 * 1024, // 500MB
                    MaxPerUpload = 10,
                    DisplayName = "STL File",
                    Description = "Stereolithography file format for 3D printing",
                    MimeType = "application/octet-stream",
                    RequiresPreview = true,
                    IsCompressible = true,
                    Category = "3D",
                    Priority = 1,
                    IsDefault = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedById = adminUser.Id,
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedById = adminUser.Id
                },
                new FileTypeSettings
                {
                    Id = Guid.NewGuid(),
                    FileExtension = ".obj",
                    Enabled = true,
                    MaxFileSizeBytes = 500L * 1024 * 1024, // 500MB
                    MaxPerUpload = 10,
                    DisplayName = "OBJ File",
                    Description = "Wavefront OBJ file format for 3D models",
                    MimeType = "text/plain",
                    RequiresPreview = true,
                    IsCompressible = true,
                    Category = "3D",
                    Priority = 2,
                    IsDefault = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedById = adminUser.Id,
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedById = adminUser.Id
                },
                new FileTypeSettings
                {
                    Id = Guid.NewGuid(),
                    FileExtension = ".fbx",
                    Enabled = true,
                    MaxFileSizeBytes = 1L * 1024 * 1024 * 1024, // 1GB
                    MaxPerUpload = 5,
                    DisplayName = "FBX File",
                    Description = "Autodesk FBX file format for 3D models and animations",
                    MimeType = "application/octet-stream",
                    RequiresPreview = true,
                    IsCompressible = true,
                    Category = "3D",
                    Priority = 3,
                    IsDefault = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedById = adminUser.Id,
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedById = adminUser.Id
                },
                new FileTypeSettings
                {
                    Id = Guid.NewGuid(),
                    FileExtension = ".gltf",
                    Enabled = true,
                    MaxFileSizeBytes = 500L * 1024 * 1024, // 500MB
                    MaxPerUpload = 10,
                    DisplayName = "GLTF File",
                    Description = "GL Transmission Format for 3D scenes and models",
                    MimeType = "model/gltf+json",
                    RequiresPreview = true,
                    IsCompressible = true,
                    Category = "3D",
                    Priority = 4,
                    IsDefault = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedById = adminUser.Id,
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedById = adminUser.Id
                },
                new FileTypeSettings
                {
                    Id = Guid.NewGuid(),
                    FileExtension = ".glb",
                    Enabled = true,
                    MaxFileSizeBytes = 500L * 1024 * 1024, // 500MB
                    MaxPerUpload = 10,
                    DisplayName = "GLB File",
                    Description = "Binary GL Transmission Format for 3D scenes and models",
                    MimeType = "model/gltf-binary",
                    RequiresPreview = true,
                    IsCompressible = true,
                    Category = "3D",
                    Priority = 5,
                    IsDefault = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedById = adminUser.Id,
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedById = adminUser.Id
                },
                new FileTypeSettings
                {
                    Id = Guid.NewGuid(),
                    FileExtension = ".3mf",
                    Enabled = true,
                    MaxFileSizeBytes = 500L * 1024 * 1024, // 500MB
                    MaxPerUpload = 10,
                    DisplayName = "3MF File",
                    Description = "3D Manufacturing Format for 3D printing",
                    MimeType = "application/vnd.ms-package.3dmanufacturing-3dmodel+xml",
                    RequiresPreview = true,
                    IsCompressible = true,
                    Category = "3D",
                    Priority = 6,
                    IsDefault = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedById = adminUser.Id,
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedById = adminUser.Id
                },
                new FileTypeSettings
                {
                    Id = Guid.NewGuid(),
                    FileExtension = ".dae",
                    Enabled = true,
                    MaxFileSizeBytes = 500L * 1024 * 1024, // 500MB
                    MaxPerUpload = 10,
                    DisplayName = "DAE File",
                    Description = "COLLADA Digital Asset Exchange format",
                    MimeType = "model/vnd.collada+xml",
                    RequiresPreview = true,
                    IsCompressible = true,
                    Category = "3D",
                    Priority = 7,
                    IsDefault = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedById = adminUser.Id,
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedById = adminUser.Id
                },
                new FileTypeSettings
                {
                    Id = Guid.NewGuid(),
                    FileExtension = ".blend",
                    Enabled = true,
                    MaxFileSizeBytes = 1L * 1024 * 1024 * 1024, // 1GB
                    MaxPerUpload = 5,
                    DisplayName = "Blend File",
                    Description = "Blender project file format",
                    MimeType = "application/x-blender",
                    RequiresPreview = true,
                    IsCompressible = true,
                    Category = "3D",
                    Priority = 8,
                    IsDefault = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedById = adminUser.Id,
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedById = adminUser.Id
                },
                new FileTypeSettings
                {
                    Id = Guid.NewGuid(),
                    FileExtension = ".max",
                    Enabled = true,
                    MaxFileSizeBytes = 1L * 1024 * 1024 * 1024, // 1GB
                    MaxPerUpload = 5,
                    DisplayName = "3DS Max File",
                    Description = "3D Studio Max project file",
                    MimeType = "application/x-3dsmax",
                    RequiresPreview = true,
                    IsCompressible = true,
                    Category = "3D",
                    Priority = 9,
                    IsDefault = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedById = adminUser.Id,
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedById = adminUser.Id
                },
                new FileTypeSettings
                {
                    Id = Guid.NewGuid(),
                    FileExtension = ".ma",
                    Enabled = true,
                    MaxFileSizeBytes = 1L * 1024 * 1024 * 1024, // 1GB
                    MaxPerUpload = 5,
                    DisplayName = "Maya ASCII File",
                    Description = "Autodesk Maya ASCII project file",
                    MimeType = "application/x-maya",
                    RequiresPreview = true,
                    IsCompressible = true,
                    Category = "3D",
                    Priority = 10,
                    IsDefault = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedById = adminUser.Id,
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedById = adminUser.Id
                },
                new FileTypeSettings
                {
                    Id = Guid.NewGuid(),
                    FileExtension = ".mb",
                    Enabled = true,
                    MaxFileSizeBytes = 1L * 1024 * 1024 * 1024, // 1GB
                    MaxPerUpload = 5,
                    DisplayName = "Maya Binary File",
                    Description = "Autodesk Maya binary project file",
                    MimeType = "application/x-maya",
                    RequiresPreview = true,
                    IsCompressible = true,
                    Category = "3D",
                    Priority = 11,
                    IsDefault = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedById = adminUser.Id,
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedById = adminUser.Id
                },
                new FileTypeSettings
                {
                    Id = Guid.NewGuid(),
                    FileExtension = ".c4d",
                    Enabled = true,
                    MaxFileSizeBytes = 1L * 1024 * 1024 * 1024, // 1GB
                    MaxPerUpload = 5,
                    DisplayName = "Cinema 4D File",
                    Description = "Cinema 4D project file",
                    MimeType = "application/x-cinema4d",
                    RequiresPreview = true,
                    IsCompressible = true,
                    Category = "3D",
                    Priority = 12,
                    IsDefault = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedById = adminUser.Id,
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedById = adminUser.Id
                },

                // Image Formats
                new FileTypeSettings
                {
                    Id = Guid.NewGuid(),
                    FileExtension = ".jpg",
                    Enabled = true,
                    MaxFileSizeBytes = 50L * 1024 * 1024, // 50MB
                    MaxPerUpload = 20,
                    DisplayName = "JPEG Image",
                    Description = "Joint Photographic Experts Group image format",
                    MimeType = "image/jpeg",
                    RequiresPreview = true,
                    IsCompressible = false,
                    Category = "Image",
                    Priority = 1,
                    IsDefault = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedById = adminUser.Id,
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedById = adminUser.Id
                },
                new FileTypeSettings
                {
                    Id = Guid.NewGuid(),
                    FileExtension = ".jpeg",
                    Enabled = true,
                    MaxFileSizeBytes = 50L * 1024 * 1024, // 50MB
                    MaxPerUpload = 20,
                    DisplayName = "JPEG Image",
                    Description = "Joint Photographic Experts Group image format",
                    MimeType = "image/jpeg",
                    RequiresPreview = true,
                    IsCompressible = false,
                    Category = "Image",
                    Priority = 2,
                    IsDefault = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedById = adminUser.Id,
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedById = adminUser.Id
                },
                new FileTypeSettings
                {
                    Id = Guid.NewGuid(),
                    FileExtension = ".png",
                    Enabled = true,
                    MaxFileSizeBytes = 100L * 1024 * 1024, // 100MB
                    MaxPerUpload = 20,
                    DisplayName = "PNG Image",
                    Description = "Portable Network Graphics image format",
                    MimeType = "image/png",
                    RequiresPreview = true,
                    IsCompressible = false,
                    Category = "Image",
                    Priority = 3,
                    IsDefault = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedById = adminUser.Id,
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedById = adminUser.Id
                },
                new FileTypeSettings
                {
                    Id = Guid.NewGuid(),
                    FileExtension = ".gif",
                    Enabled = true,
                    MaxFileSizeBytes = 50L * 1024 * 1024, // 50MB
                    MaxPerUpload = 20,
                    DisplayName = "GIF Image",
                    Description = "Graphics Interchange Format image",
                    MimeType = "image/gif",
                    RequiresPreview = true,
                    IsCompressible = false,
                    Category = "Image",
                    Priority = 4,
                    IsDefault = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedById = adminUser.Id,
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedById = adminUser.Id
                },
                new FileTypeSettings
                {
                    Id = Guid.NewGuid(),
                    FileExtension = ".bmp",
                    Enabled = true,
                    MaxFileSizeBytes = 100L * 1024 * 1024, // 100MB
                    MaxPerUpload = 20,
                    DisplayName = "BMP Image",
                    Description = "Bitmap image format",
                    MimeType = "image/bmp",
                    RequiresPreview = true,
                    IsCompressible = false,
                    Category = "Image",
                    Priority = 5,
                    IsDefault = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedById = adminUser.Id,
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedById = adminUser.Id
                },
                new FileTypeSettings
                {
                    Id = Guid.NewGuid(),
                    FileExtension = ".tiff",
                    Enabled = true,
                    MaxFileSizeBytes = 200L * 1024 * 1024, // 200MB
                    MaxPerUpload = 15,
                    DisplayName = "TIFF Image",
                    Description = "Tagged Image File Format",
                    MimeType = "image/tiff",
                    RequiresPreview = true,
                    IsCompressible = false,
                    Category = "Image",
                    Priority = 6,
                    IsDefault = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedById = adminUser.Id,
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedById = adminUser.Id
                },
                new FileTypeSettings
                {
                    Id = Guid.NewGuid(),
                    FileExtension = ".tga",
                    Enabled = true,
                    MaxFileSizeBytes = 100L * 1024 * 1024, // 100MB
                    MaxPerUpload = 20,
                    DisplayName = "TGA Image",
                    Description = "Truevision Graphics Adapter image format",
                    MimeType = "image/x-tga",
                    RequiresPreview = true,
                    IsCompressible = false,
                    Category = "Image",
                    Priority = 7,
                    IsDefault = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedById = adminUser.Id,
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedById = adminUser.Id
                },
                new FileTypeSettings
                {
                    Id = Guid.NewGuid(),
                    FileExtension = ".hdr",
                    Enabled = true,
                    MaxFileSizeBytes = 200L * 1024 * 1024, // 200MB
                    MaxPerUpload = 15,
                    DisplayName = "HDR Image",
                    Description = "High Dynamic Range image format",
                    MimeType = "image/vnd.radiance",
                    RequiresPreview = true,
                    IsCompressible = false,
                    Category = "Image",
                    Priority = 8,
                    IsDefault = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedById = adminUser.Id,
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedById = adminUser.Id
                },
                new FileTypeSettings
                {
                    Id = Guid.NewGuid(),
                    FileExtension = ".exr",
                    Enabled = true,
                    MaxFileSizeBytes = 200L * 1024 * 1024, // 200MB
                    MaxPerUpload = 15,
                    DisplayName = "EXR Image",
                    Description = "OpenEXR high dynamic range image format",
                    MimeType = "image/x-exr",
                    RequiresPreview = true,
                    IsCompressible = false,
                    Category = "Image",
                    Priority = 9,
                    IsDefault = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedById = adminUser.Id,
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedById = adminUser.Id
                },

                // Document Formats
                new FileTypeSettings
                {
                    Id = Guid.NewGuid(),
                    FileExtension = ".md",
                    Enabled = true,
                    MaxFileSizeBytes = 10L * 1024 * 1024, // 10MB
                    MaxPerUpload = 50,
                    DisplayName = "Markdown File",
                    Description = "Markdown text file format",
                    MimeType = "text/markdown",
                    RequiresPreview = true,
                    IsCompressible = true,
                    Category = "Document",
                    Priority = 1,
                    IsDefault = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedById = adminUser.Id,
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedById = adminUser.Id
                },
                new FileTypeSettings
                {
                    Id = Guid.NewGuid(),
                    FileExtension = ".markdown",
                    Enabled = true,
                    MaxFileSizeBytes = 10L * 1024 * 1024, // 10MB
                    MaxPerUpload = 50,
                    DisplayName = "Markdown File",
                    Description = "Markdown text file format",
                    MimeType = "text/markdown",
                    RequiresPreview = true,
                    IsCompressible = true,
                    Category = "Document",
                    Priority = 2,
                    IsDefault = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedById = adminUser.Id,
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedById = adminUser.Id
                },
                new FileTypeSettings
                {
                    Id = Guid.NewGuid(),
                    FileExtension = ".pdf",
                    Enabled = true,
                    MaxFileSizeBytes = 100L * 1024 * 1024, // 100MB
                    MaxPerUpload = 20,
                    DisplayName = "PDF Document",
                    Description = "Portable Document Format",
                    MimeType = "application/pdf",
                    RequiresPreview = true,
                    IsCompressible = false,
                    Category = "Document",
                    Priority = 3,
                    IsDefault = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedById = adminUser.Id,
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedById = adminUser.Id
                },
                new FileTypeSettings
                {
                    Id = Guid.NewGuid(),
                    FileExtension = ".txt",
                    Enabled = true,
                    MaxFileSizeBytes = 10L * 1024 * 1024, // 10MB
                    MaxPerUpload = 50,
                    DisplayName = "Text File",
                    Description = "Plain text file format",
                    MimeType = "text/plain",
                    RequiresPreview = true,
                    IsCompressible = true,
                    Category = "Document",
                    Priority = 4,
                    IsDefault = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedById = adminUser.Id,
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedById = adminUser.Id
                },

                // Archive Formats
                new FileTypeSettings
                {
                    Id = Guid.NewGuid(),
                    FileExtension = ".zip",
                    Enabled = true,
                    MaxFileSizeBytes = 1L * 1024 * 1024 * 1024, // 1GB
                    MaxPerUpload = 5,
                    DisplayName = "ZIP Archive",
                    Description = "ZIP compressed archive format",
                    MimeType = "application/zip",
                    RequiresPreview = false,
                    IsCompressible = false,
                    Category = "Archive",
                    Priority = 1,
                    IsDefault = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedById = adminUser.Id,
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedById = adminUser.Id
                }
            };

            _context.FileTypeSettings.AddRange(fileTypes);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("File type settings seeded successfully");
        }
    }
}
