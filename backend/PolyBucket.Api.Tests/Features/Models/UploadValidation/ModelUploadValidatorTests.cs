using Microsoft.AspNetCore.Http;
using Moq;
using PolyBucket.Api.Features.Models.UploadValidation.Domain;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Models.UploadValidation;

public class ModelUploadValidatorTests
{
    private readonly Mock<IModelUploadLimitsProvider> _limitsProvider = new();
    private readonly ModelUploadValidator _validator;

    public ModelUploadValidatorTests()
    {
        _validator = new ModelUploadValidator(_limitsProvider.Object);
    }

    [Fact(DisplayName = "When file count exceeds the site limit, validation returns an error.")]
    public async Task ValidateAsync_WhenTooManyFiles_ReturnsError()
    {
        // Arrange
        _limitsProvider
            .Setup(p => p.GetSnapshotAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildSnapshot(maxFiles: 1));

        var files = new[] { CreateStlFile("a.stl"), CreateStlFile("b.stl") };

        // Act
        var result = await _validator.ValidateAsync(files, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result!.ShouldContain("Maximum of 1");
    }

    [Fact(DisplayName = "When an extension is not allowed, validation returns an error.")]
    public async Task ValidateAsync_WhenExtensionNotAllowed_ReturnsError()
    {
        // Arrange
        _limitsProvider
            .Setup(p => p.GetSnapshotAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildSnapshot());

        var files = new[] { CreateTextFile("notes.txt") };

        // Act
        var result = await _validator.ValidateAsync(files, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result!.ShouldContain("Unsupported file type");
    }

    [Fact(DisplayName = "When a file exceeds the effective size cap, validation returns an error.")]
    public async Task ValidateAsync_WhenFileTooLarge_ReturnsError()
    {
        // Arrange
        _limitsProvider
            .Setup(p => p.GetSnapshotAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildSnapshot(siteMaxBytes: 50, perTypeMaxBytes: 100));

        var files = new[] { CreateStlFile("large.stl", sizeBytes: 80) };

        // Act
        var result = await _validator.ValidateAsync(files, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result!.ShouldContain("too large");
    }

    [Fact(DisplayName = "When no 3D category file is present, validation returns an error.")]
    public async Task ValidateAsync_WhenMissing3D_ReturnsError()
    {
        // Arrange
        _limitsProvider
            .Setup(p => p.GetSnapshotAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ModelUploadLimitsSnapshot
            {
                SiteMaxFileSizeBytes = 100 * 1024 * 1024,
                MaxFilesPerUpload = 5,
                EnabledFileTypes = new[]
                {
                    new ModelUploadFileTypeLimit
                    {
                        Extension = ".png",
                        Category = "Image",
                        MaxFileSizeBytes = 100 * 1024 * 1024
                    }
                },
                ExtensionCategories = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    [".png"] = "Image"
                }
            });

        var pngBytes = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00 };
        var files = new[] { CreateFormFile("thumb.png", pngBytes, "image/png") };

        // Act
        var result = await _validator.ValidateAsync(files, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result!.ShouldContain("3D model file");
    }

    [Fact(DisplayName = "When a DXF file is enabled, validation accepts it as a 3D upload.")]
    public async Task ValidateAsync_WhenDxfEnabled_ReturnsNull()
    {
        // Arrange
        _limitsProvider
            .Setup(p => p.GetSnapshotAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ModelUploadLimitsSnapshot
            {
                SiteMaxFileSizeBytes = 100 * 1024 * 1024,
                MaxFilesPerUpload = 5,
                EnabledFileTypes = new[]
                {
                    new ModelUploadFileTypeLimit
                    {
                        Extension = ".dxf",
                        Category = "3D",
                        MaxFileSizeBytes = 100 * 1024 * 1024
                    }
                },
                ExtensionCategories = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    [".dxf"] = "3D"
                }
            });

        var content = "  0\nSECTION\n  2\nHEADER\n";
        var files = new[] { CreateFormFile("bracket.dxf", System.Text.Encoding.UTF8.GetBytes(content), "image/vnd.dxf") };

        // Act
        var result = await _validator.ValidateAsync(files, CancellationToken.None);

        // Assert
        result.ShouldBeNull();
    }

  [Fact(DisplayName = "When files meet limits and include a 3D file, validation succeeds.")]
    public async Task ValidateAsync_WhenValid_ReturnsNull()
    {
        // Arrange
        _limitsProvider
            .Setup(p => p.GetSnapshotAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildSnapshot());

        var files = new[] { CreateStlFile("model.stl") };

        // Act
        var result = await _validator.ValidateAsync(files, CancellationToken.None);

        // Assert
        result.ShouldBeNull();
    }

    private static ModelUploadLimitsSnapshot BuildSnapshot(
        int maxFiles = 5,
        long siteMaxBytes = 100 * 1024 * 1024,
        long perTypeMaxBytes = 100 * 1024 * 1024)
    {
        return new ModelUploadLimitsSnapshot
        {
            SiteMaxFileSizeBytes = siteMaxBytes,
            MaxFilesPerUpload = maxFiles,
            EnabledFileTypes = new[]
            {
                new ModelUploadFileTypeLimit
                {
                    Extension = ".stl",
                    Category = "3D",
                    MaxFileSizeBytes = perTypeMaxBytes
                }
            },
            ExtensionCategories = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                [".stl"] = "3D"
            }
        };
    }

    private static IFormFile CreateStlFile(string name, int sizeBytes = 32)
    {
        var content = "solid polybucket-test\nendsolid polybucket-test\n";
        var bytes = System.Text.Encoding.UTF8.GetBytes(content);
        if (bytes.Length < sizeBytes)
        {
            bytes = bytes.Concat(new byte[sizeBytes - bytes.Length]).ToArray();
        }

        return CreateFormFile(name, bytes, "application/octet-stream");
    }

    private static IFormFile CreateTextFile(string name)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes("hello");
        return CreateFormFile(name, bytes, "text/plain");
    }

    private static IFormFile CreateFormFile(string name, byte[] bytes, string contentType)
    {
        var mock = new Mock<IFormFile>();
        mock.Setup(f => f.FileName).Returns(name);
        mock.Setup(f => f.Length).Returns(bytes.Length);
        mock.Setup(f => f.ContentType).Returns(contentType);
        mock.Setup(f => f.OpenReadStream()).Returns(() => new MemoryStream(bytes));
        return mock.Object;
    }
}