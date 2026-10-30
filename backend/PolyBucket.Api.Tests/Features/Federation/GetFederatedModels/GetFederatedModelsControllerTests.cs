using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Federation.GetFederatedModels.Domain;
using PolyBucket.Api.Features.Federation.GetFederatedModels.Http;
using PolyBucket.Api.Tests.Testing;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Federation.GetFederatedModels;

public class GetFederatedModelsControllerTests
{
    private readonly Mock<IGetFederatedModelsService> _service = new();

    [Fact]
    public async Task GetModels_ReturnsOk()
    {
        _service.Setup(s => s.GetFederatedModelsAsync(It.IsAny<string?>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((new OkObjectResult(new List<FederatedModelDto>()), 0));
        var controller = new GetFederatedModelsController(_service.Object).WithUser(Guid.NewGuid());
        var result = await controller.GetFederatedModels();
        result.Result.ShouldBeOfType<OkObjectResult>();
    }
}
