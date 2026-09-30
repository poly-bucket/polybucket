using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Federation.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Federation.InitiateHandshake;

public class InitiateHandshakeControllerTests : IDisposable
{
    private readonly PolyBucketDbContext _context;

    public InitiateHandshakeControllerTests()
    {
        _context = new PolyBucketDbContext(new DbContextOptionsBuilder<PolyBucketDbContext>()
            .UseInMemoryDatabase(global::System.Guid.NewGuid().ToString())
            .Options);
    }

    [Fact(DisplayName = "When initiator URL is missing, InitiateHandshake returns BadRequest.")]
    public async Task InitiateHandshake_MissingUrl_ReturnsBadRequest()
    {
        // Arrange
        var factory = new Mock<IHttpClientFactory>();
        var config = new Mock<IConfiguration>();
        var controller = new InitiateHandshakeController(_context, factory.Object, config.Object);

        // Act
        var result = await controller.InitiateHandshake(new InitiateHandshakeRequest());

        // Assert
        result.Result.ShouldBeOfType<BadRequestObjectResult>();
    }

    public void Dispose() => _context.Dispose();
}
