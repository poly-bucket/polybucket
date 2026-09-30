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

namespace PolyBucket.Tests.Features.Federation.GetFederatedInstance;

public class GetFederatedInstanceControllerTests
{
    private readonly Mock<IFederationRepository> _repository = new();

    [Fact(DisplayName = "When the instance exists, GetFederatedInstance returns Ok.")]
    public async Task GetFederatedInstance_Found_ReturnsOk()
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
        var controller = new GetFederatedInstanceController(_repository.Object).WithUser(Guid.NewGuid());

        // Act
        var result = await controller.GetFederatedInstance(id);

        // Assert
        result.Result.ShouldBeOfType<OkObjectResult>();
    }

    [Fact(DisplayName = "When the instance is missing, GetFederatedInstance returns NotFound.")]
    public async Task GetFederatedInstance_Missing_ReturnsNotFound()
    {
        // Arrange
        _repository.Setup(r => r.GetFederatedInstanceAsync(It.IsAny<Guid>())).ReturnsAsync((FederatedInstance?)null);
        var controller = new GetFederatedInstanceController(_repository.Object).WithUser(Guid.NewGuid());

        // Act
        var result = await controller.GetFederatedInstance(Guid.NewGuid());

        // Assert
        result.Result.ShouldBeOfType<NotFoundObjectResult>();
    }
}
