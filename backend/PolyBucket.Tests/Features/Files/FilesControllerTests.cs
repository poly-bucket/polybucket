using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Files.Http;
using PolyBucket.Api.Features.SystemSettings.Domain;
using PolyBucket.Api.Settings;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Files;

public class FilesControllerTests : IDisposable
{
    private readonly PolyBucketDbContext _context;

    public FilesControllerTests()
    {
        _context = new PolyBucketDbContext(new DbContextOptionsBuilder<PolyBucketDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);
        _context.FileTypeSettings.Add(new FileTypeSettings
        {
            FileExtension = ".stl",
            DisplayName = "STL",
            Description = "Mesh",
            MimeType = "model/stl",
            MaxFileSizeBytes = 10_000_000,
            MaxPerUpload = 5,
            Enabled = true,
            Category = "3d",
            Priority = 1
        });
        _context.SaveChanges();
    }

    [Fact(DisplayName = "When file config is requested, GetFileConfig returns Ok with extensions.")]
    public async Task GetFileConfig_ReturnsOk()
    {
        // Arrange
        var controller = new GetFileConfigController(_context, Options.Create(new StorageSettings { Provider = "local" }));

        // Act
        var result = await controller.GetFileConfig();

        // Assert
        var ok = result.Result.ShouldBeOfType<OkObjectResult>();
        var config = ok.Value.ShouldBeOfType<FileConfigResponse>();
        config.StorageProvider.ShouldBe("local");
        config.SupportedExtensions.ShouldContain(".stl");
    }

    [Fact(DisplayName = "When supported extensions are requested, GetSupportedExtensions returns Ok.")]
    public async Task GetSupportedExtensions_ReturnsOk()
    {
        // Arrange
        var controller = new GetSupportedExtensionsController(_context);

        // Act
        var result = await controller.GetSupportedExtensions();

        // Assert
        var ok = result.Result.ShouldBeOfType<OkObjectResult>();
        var extensions = ok.Value.ShouldBeOfType<System.Collections.Generic.List<string>>();
        extensions.ShouldContain(".stl");
    }

    [Fact(DisplayName = "When extensions by type are requested, GetSupportedExtensionsByType returns Ok.")]
    public async Task GetSupportedExtensionsByType_ReturnsOk()
    {
        // Arrange
        var controller = new GetSupportedExtensionsByTypeController(_context);

        // Act
        var result = await controller.GetSupportedExtensionsForType("3d");

        // Assert
        result.Result.ShouldBeOfType<OkObjectResult>();
    }

    public void Dispose() => _context.Dispose();
}
