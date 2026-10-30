using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Federation.UpdateFederatedInstance.Domain;
using PolyBucket.Api.Features.Federation.UpdateFederatedInstance.Http;
using PolyBucket.Api.Tests.Testing;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Federation.UpdateFederatedInstance;

public class UpdateFederatedInstanceControllerTests
{
    private readonly Mock<IUpdateFederatedInstanceService> _service = new();

    [Fact]
    public async Task Update_WhenMissing_ReturnsNotFound()
    {
        _service.Setup(s => s.UpdateFederatedInstanceAsync(It.IsAny<Guid>(), It.IsAny<UpdateFederatedInstanceRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new NotFoundObjectResult("missing"));
        var controller = new UpdateFederatedInstanceController(_service.Object).WithUser(Guid.NewGuid());
        var result = await controller.UpdateFederatedInstance(Guid.NewGuid(), new UpdateFederatedInstanceRequest());
        result.Result.ShouldBeOfType<NotFoundObjectResult>();
    }
}
