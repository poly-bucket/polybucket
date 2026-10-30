using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Federation.GetFederatedInstance.Domain;
using PolyBucket.Api.Features.Federation.GetFederatedInstance.Http;
using PolyBucket.Api.Tests.Testing;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Federation.GetFederatedInstance;

public class GetFederatedInstanceControllerTests
{
    private readonly Mock<IGetFederatedInstanceService> _service = new();

    [Fact(DisplayName = "When the instance exists, GetFederatedInstance returns Ok.")]
    public async Task GetFederatedInstance_Found_ReturnsOk()
    {
        var id = Guid.NewGuid();
        _service.Setup(s => s.GetAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(new FederatedInstanceDetailDto
        {
            Id = id,
            Name = "Peer",
            BaseUrl = "https://peer.example",
            Status = "Active",
            CreatedAt = DateTime.UtcNow
        });
        var controller = new GetFederatedInstanceController(_service.Object).WithUser(Guid.NewGuid());

        var result = await controller.GetFederatedInstance(id);

        result.Result.ShouldBeOfType<OkObjectResult>();
    }

    [Fact(DisplayName = "When the instance is missing, GetFederatedInstance returns NotFound.")]
    public async Task GetFederatedInstance_Missing_ReturnsNotFound()
    {
        _service.Setup(s => s.GetAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((FederatedInstanceDetailDto?)null);
        var controller = new GetFederatedInstanceController(_service.Object).WithUser(Guid.NewGuid());

        var result = await controller.GetFederatedInstance(Guid.NewGuid());

        result.Result.ShouldBeOfType<NotFoundObjectResult>();
    }
}
