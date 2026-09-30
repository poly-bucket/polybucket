using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Federation.Domain;
using PolyBucket.Api.Features.Federation.Http;
using PolyBucket.Api.Features.Federation.Repository;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Federation.UpdateFederatedInstance;

public class UpdateFederatedInstanceControllerTests
{
    private readonly Mock<IFederationRepository> _repository = new();

    [Fact(DisplayName = "When the instance is missing, UpdateFederatedInstance returns NotFound.")]
    public async Task UpdateFederatedInstance_Missing_ReturnsNotFound()
    {
        // Arrange
        _repository.Setup(r => r.GetFederatedInstanceAsync(It.IsAny<Guid>())).ReturnsAsync((FederatedInstance?)null);
        var accessor = new Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
        var controller = new UpdateFederatedInstanceController(_repository.Object, accessor.Object);

        // Act
        var result = await controller.UpdateFederatedInstance(Guid.NewGuid(), new UpdateFederatedInstanceRequest { Name = "X" });

        // Assert
        result.Result.ShouldBeOfType<NotFoundObjectResult>();
    }

    [Fact(DisplayName = "When the base URL is invalid, UpdateFederatedInstance returns BadRequest.")]
    public async Task UpdateFederatedInstance_InvalidUrl_ReturnsBadRequest()
    {
        // Arrange
        var id = Guid.NewGuid();
        _repository.Setup(r => r.GetFederatedInstanceAsync(id)).ReturnsAsync(new FederatedInstance
        {
            Id = id,
            Name = "Peer",
            BaseUrl = "https://peer.example",
            Status = FederationStatus.Active,
            CreatedAt = DateTime.UtcNow
        });
        var accessor = new Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
        var controller = new UpdateFederatedInstanceController(_repository.Object, accessor.Object);

        // Act
        var result = await controller.UpdateFederatedInstance(id, new UpdateFederatedInstanceRequest { BaseUrl = "not-a-url" });

        // Assert
        result.Result.ShouldBeOfType<BadRequestObjectResult>();
    }
}
