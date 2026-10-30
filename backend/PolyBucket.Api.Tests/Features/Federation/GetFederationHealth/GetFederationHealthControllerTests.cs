using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Federation.GetFederationHealth.Domain;
using PolyBucket.Api.Features.Federation.GetFederationHealth.Http;
using PolyBucket.Api.Tests.Testing;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Federation.GetFederationHealth;

public class GetFederationHealthControllerTests
{
    private readonly Mock<IGetFederationHealthService> _service = new();

    [Fact]
    public async Task GetHealth_WhenMissing_ReturnsNotFound()
    {
        _service.Setup(s => s.GetFederationHealthAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new NotFoundObjectResult("missing"));
        var controller = new GetFederationHealthController(_service.Object).WithUser(Guid.NewGuid());
        var result = await controller.GetFederationHealth(Guid.NewGuid());
        result.Result.ShouldBeOfType<NotFoundObjectResult>();
    }
}
