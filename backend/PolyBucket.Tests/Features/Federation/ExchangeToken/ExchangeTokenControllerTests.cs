using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Federation.Http;
using PolyBucket.Api.Features.Federation.Services;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Federation.ExchangeToken;

public class ExchangeTokenControllerTests : IDisposable
{
    private readonly PolyBucketDbContext _context;

    public ExchangeTokenControllerTests()
    {
        _context = new PolyBucketDbContext(new DbContextOptionsBuilder<PolyBucketDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);
    }

    [Fact(DisplayName = "When handshake id is empty, ExchangeToken returns BadRequest.")]
    public async Task ExchangeToken_EmptyHandshake_ReturnsBadRequest()
    {
        // Arrange
        var tokenService = new Mock<IFederationTokenService>();
        var config = new Mock<IConfiguration>();
        var controller = new ExchangeTokenController(_context, tokenService.Object, config.Object);

        // Act
        var result = await controller.ExchangeToken(new ExchangeTokenRequest { HandshakeId = Guid.Empty });

        // Assert
        result.Result.ShouldBeOfType<BadRequestObjectResult>();
    }

    public void Dispose() => _context.Dispose();
}
