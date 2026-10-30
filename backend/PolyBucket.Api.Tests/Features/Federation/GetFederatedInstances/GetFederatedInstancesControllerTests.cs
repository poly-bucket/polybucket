using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Federation.GetFederatedInstances.Domain;
using PolyBucket.Api.Features.Federation.GetFederatedInstances.Http;
using PolyBucket.Api.Tests.Testing;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Federation.GetFederatedInstances;

public class GetFederatedInstancesControllerTests
{
    private readonly Mock<IGetFederatedInstancesService> _service = new();

    [Fact]
    public async Task GetFederatedInstances_ReturnsOk()
    {
        _service.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Array.Empty<FederatedInstanceDto>());
        var controller = new GetFederatedInstancesController(_service.Object).WithUser(Guid.NewGuid());
        var result = await controller.GetFederatedInstances();
        result.Result.ShouldBeOfType<OkObjectResult>();
    }
}
