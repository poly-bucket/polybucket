using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Federation.Domain;
using PolyBucket.Api.Features.Federation.Http;
using PolyBucket.Api.Features.Federation.Repository;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Federation.CreateFederatedInstance;

public class CreateFederatedInstanceControllerTests
{
    private readonly Mock<IFederationRepository> _repository = new();
    private readonly Mock<IHttpContextAccessor> _httpContextAccessor = new();
    private readonly Guid _userId = Guid.NewGuid();

    private CreateFederatedInstanceController CreateController(bool authenticated = true)
    {
        var context = new DefaultHttpContext();
        if (authenticated)
        {
            context.User = new ClaimsPrincipal(new ClaimsIdentity(
                new[] { new Claim(ClaimTypes.NameIdentifier, _userId.ToString()) }, "Test"));
        }
        _httpContextAccessor.Setup(a => a.HttpContext).Returns(context);
        return new CreateFederatedInstanceController(_repository.Object, _httpContextAccessor.Object);
    }

    [Fact(DisplayName = "When the request is invalid, CreateFederatedInstance returns BadRequest.")]
    public async Task CreateFederatedInstance_MissingName_ReturnsBadRequest()
    {
        // Act
        var result = await CreateController().CreateFederatedInstance(new CreateFederatedInstanceRequest { BaseUrl = "https://x.com" });

        // Assert
        result.Result.ShouldBeOfType<BadRequestObjectResult>();
    }

    [Fact(DisplayName = "When the user is not authenticated, CreateFederatedInstance returns Unauthorized.")]
    public async Task CreateFederatedInstance_NoUser_ReturnsUnauthorized()
    {
        // Act
        var result = await CreateController(authenticated: false).CreateFederatedInstance(new CreateFederatedInstanceRequest
        {
            Name = "Peer",
            BaseUrl = "https://peer.example"
        });

        // Assert
        result.Result.ShouldBeOfType<UnauthorizedObjectResult>();
    }

    [Fact(DisplayName = "When the request is valid, CreateFederatedInstance returns 201.")]
    public async Task CreateFederatedInstance_Valid_ReturnsCreated()
    {
        // Arrange
        var created = new FederatedInstance
        {
            Id = Guid.NewGuid(),
            Name = "Peer",
            BaseUrl = "https://peer.example",
            Status = FederationStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
        _repository.Setup(r => r.AddFederatedInstanceAsync(It.IsAny<FederatedInstance>())).ReturnsAsync(created);

        // Act
        var result = await CreateController().CreateFederatedInstance(new CreateFederatedInstanceRequest
        {
            Name = "Peer",
            BaseUrl = "https://peer.example"
        });

        // Assert
        result.Result.ShouldBeOfType<CreatedAtActionResult>();
    }
}
