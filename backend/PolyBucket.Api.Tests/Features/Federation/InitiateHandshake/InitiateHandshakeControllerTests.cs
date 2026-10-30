using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Federation.InitiateHandshake.Domain;
using PolyBucket.Api.Features.Federation.InitiateHandshake.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Federation.InitiateHandshake;

public class InitiateHandshakeControllerTests
{
    private readonly Mock<IInitiateHandshakeService> _service = new();

    [Fact]
    public async Task Initiate_WhenInvalid_ReturnsBadRequest()
    {
        _service.Setup(s => s.InitiateHandshakeAsync(It.IsAny<InitiateHandshakeRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BadRequestObjectResult("bad"));
        var controller = new InitiateHandshakeController(_service.Object);
        var result = await controller.InitiateHandshake(new InitiateHandshakeRequest());
        result.Result.ShouldBeOfType<BadRequestObjectResult>();
    }
}
