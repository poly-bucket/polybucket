using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Federation.Domain;
using PolyBucket.Api.Features.Federation.Http;
using PolyBucket.Api.Features.Federation.Repository;
using PolyBucket.Tests.Testing;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Federation.GetFederatedInstances;

public class GetFederatedInstancesControllerTests
{
    private readonly Mock<IFederationRepository> _repository = new();

    [Fact(DisplayName = "When instances exist, GetFederatedInstances returns Ok with DTOs.")]
    public async Task GetFederatedInstances_ReturnsOk()
    {
        // Arrange
        var instance = new FederatedInstance
        {
            Id = Guid.NewGuid(),
            Name = "Remote",
            BaseUrl = "https://remote.example",
            Status = FederationStatus.Active,
            CreatedAt = DateTime.UtcNow
        };
        _repository.Setup(r => r.GetFederatedInstancesAsync()).ReturnsAsync(new List<FederatedInstance> { instance });
        var controller = new GetFederatedInstancesController(_repository.Object).WithUser(Guid.NewGuid());

        // Act
        var result = await controller.GetFederatedInstances();

        // Assert
        var ok = result.Result.ShouldBeOfType<OkObjectResult>();
        var list = ok.Value.ShouldBeOfType<List<FederatedInstanceDto>>();
        list.ShouldHaveSingleItem().Name.ShouldBe("Remote");
    }
}
