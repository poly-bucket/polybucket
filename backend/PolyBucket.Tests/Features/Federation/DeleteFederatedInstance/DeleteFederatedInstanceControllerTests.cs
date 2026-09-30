using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Federation.Domain;
using PolyBucket.Api.Features.Federation.Http;
using PolyBucket.Api.Features.Federation.Repository;
using PolyBucket.Tests.Testing;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Federation.DeleteFederatedInstance;

public class DeleteFederatedInstanceControllerTests
{
    private readonly Mock<IFederationRepository> _repository = new();

    [Fact(DisplayName = "When the instance exists, DeleteFederatedInstance returns NoContent.")]
    public async Task DeleteFederatedInstance_Found_ReturnsNoContent()
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
        var controller = new DeleteFederatedInstanceController(_repository.Object).WithUser(Guid.NewGuid());

        // Act
        var result = await controller.DeleteFederatedInstance(id);

        // Assert
        result.ShouldBeOfType<NoContentResult>();
        _repository.Verify(r => r.DeleteFederatedInstanceAsync(id), Times.Once);
    }

    [Fact(DisplayName = "When the instance is missing, DeleteFederatedInstance returns NotFound.")]
    public async Task DeleteFederatedInstance_Missing_ReturnsNotFound()
    {
        // Arrange
        _repository.Setup(r => r.GetFederatedInstanceAsync(It.IsAny<Guid>())).ReturnsAsync((FederatedInstance?)null);
        var controller = new DeleteFederatedInstanceController(_repository.Object).WithUser(Guid.NewGuid());

        // Act
        var result = await controller.DeleteFederatedInstance(Guid.NewGuid());

        // Assert
        result.ShouldBeOfType<NotFoundObjectResult>();
    }
}
