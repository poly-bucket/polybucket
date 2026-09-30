using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.ACL.Services;
using PolyBucket.Api.Features.Files.Http;
using PolyBucket.Api.Features.Models.RecordModelDownload.Domain;
using PolyBucket.Api.Common.Storage;
using PolyBucket.Api.Settings;
using PolyBucket.Tests.Testing;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Files;

public class StreamFileControllerTests : IDisposable
{
    private readonly PolyBucketDbContext _context;

    public StreamFileControllerTests()
    {
        _context = new PolyBucketDbContext(new DbContextOptionsBuilder<PolyBucketDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);
    }

    [Fact(DisplayName = "When the file id is unknown, StreamFile returns NotFound.")]
    public async Task StreamFile_UnknownFile_ReturnsNotFound()
    {
        // Arrange
        var controller = new StreamFileController(
            _context,
            Mock.Of<IPermissionService>(),
            Mock.Of<IStorageService>(),
            Options.Create(new StorageSettings()),
            Mock.Of<IModelDownloadCounter>());
        controller.WithUser(Guid.NewGuid());

        // Act
        var result = await controller.StreamFile(Guid.NewGuid());

        // Assert
        result.ShouldBeOfType<NotFoundObjectResult>();
    }

    public void Dispose() => _context.Dispose();
}
