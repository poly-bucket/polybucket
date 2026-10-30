using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Federation.DeleteFederatedInstance.Domain;
using PolyBucket.Api.Features.Federation.DeleteFederatedInstance.Http;
using PolyBucket.Api.Tests.Testing;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Federation.DeleteFederatedInstance;

public class DeleteFederatedInstanceControllerTests
{
    private readonly Mock<IDeleteFederatedInstanceService> _service = new();

    [Fact]
    public async Task Delete_WhenMissing_ReturnsNotFound()
    {
        _service.Setup(s => s.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(DeleteFederatedInstanceOutcome.NotFound);
        var controller = new DeleteFederatedInstanceController(_service.Object).WithUser(Guid.NewGuid());
        var result = await controller.DeleteFederatedInstance(Guid.NewGuid());
        result.ShouldBeOfType<NotFoundObjectResult>();
    }
}
