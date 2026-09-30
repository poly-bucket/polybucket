using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Federation.Http;
using PolyBucket.Api.Features.Federation.Services;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Federation.GetCatalog;

public class GetCatalogControllerTests : IDisposable
{
    private readonly PolyBucketDbContext _context;
    private readonly Mock<IFederationTokenService> _tokenService = new();

    public GetCatalogControllerTests()
    {
        _context = new PolyBucketDbContext(new DbContextOptionsBuilder<PolyBucketDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);
    }

    private GetCatalogController CreateController(string? bearer = null)
    {
        var controller = new GetCatalogController(_context, _tokenService.Object);
        var context = new DefaultHttpContext();
        if (bearer != null)
        {
            context.Request.Headers.Authorization = $"Bearer {bearer}";
        }
        controller.ControllerContext = new ControllerContext { HttpContext = context };
        return controller;
    }

    [Fact(DisplayName = "When no federation token is provided, GetCatalog returns Unauthorized.")]
    public async Task GetCatalog_NoToken_ReturnsUnauthorized()
    {
        // Act
        var result = await CreateController().GetCatalog();

        // Assert
        result.Result.ShouldBeOfType<UnauthorizedObjectResult>();
    }

    [Fact(DisplayName = "When a federation token is provided, GetCatalog returns Ok with catalog metadata.")]
    public async Task GetCatalog_WithToken_ReturnsOk()
    {
        // Act
        var result = await CreateController("fed-token").GetCatalog();

        // Assert
        var ok = result.Result.ShouldBeOfType<OkObjectResult>();
        ok.Value.ShouldBeOfType<CatalogResponse>().TotalModels.ShouldBe(0);
    }

    public void Dispose() => _context.Dispose();
}
