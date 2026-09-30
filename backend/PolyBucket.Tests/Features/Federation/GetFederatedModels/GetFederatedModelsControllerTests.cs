using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Federation.Http;
using PolyBucket.Tests.Testing;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Federation.GetFederatedModels;

public class GetFederatedModelsControllerTests : IDisposable
{
    private readonly PolyBucketDbContext _context;

    public GetFederatedModelsControllerTests()
    {
        _context = new PolyBucketDbContext(new DbContextOptionsBuilder<PolyBucketDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);
    }

    [Fact(DisplayName = "When no federated models exist, GetFederatedModels returns Ok with an empty list.")]
    public async Task GetFederatedModels_Empty_ReturnsOk()
    {
        // Arrange
        var controller = new GetFederatedModelsController(_context).WithUser(Guid.NewGuid());

        // Act
        var result = await controller.GetFederatedModels();

        // Assert
        result.Result.ShouldBeOfType<OkObjectResult>();
    }

    public void Dispose() => _context.Dispose();
}
