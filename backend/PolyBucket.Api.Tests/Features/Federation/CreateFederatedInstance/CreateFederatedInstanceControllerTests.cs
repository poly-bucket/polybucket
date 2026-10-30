using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Federation.CreateFederatedInstance.Domain;
using PolyBucket.Api.Features.Federation.CreateFederatedInstance.Http;
using PolyBucket.Api.Tests.Testing;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Federation.CreateFederatedInstance;

public class CreateFederatedInstanceControllerTests
{
    private readonly Mock<ICreateFederatedInstanceService> _service = new();

    [Fact]
    public async Task Create_WhenInvalid_ReturnsBadRequest()
    {
        _service.Setup(s => s.CreateAsync(It.IsAny<CreateFederatedInstanceCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CreateFederatedInstanceResult { Outcome = CreateFederatedInstanceOutcome.ValidationError, ErrorMessage = "bad" });
        var controller = new CreateFederatedInstanceController(_service.Object).WithUser(Guid.NewGuid());
        var result = await controller.CreateFederatedInstance(new CreateFederatedInstanceRequest());
        result.Result.ShouldBeOfType<BadRequestObjectResult>();
    }
}
