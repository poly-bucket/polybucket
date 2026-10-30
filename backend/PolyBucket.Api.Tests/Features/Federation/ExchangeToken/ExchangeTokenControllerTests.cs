using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Federation.ExchangeToken.Domain;
using PolyBucket.Api.Features.Federation.ExchangeToken.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Federation.ExchangeToken;

public class ExchangeTokenControllerTests
{
    private readonly Mock<IExchangeTokenService> _service = new();

    [Fact]
    public async Task Exchange_WhenInvalid_ReturnsBadRequest()
    {
        _service.Setup(s => s.ExchangeTokenAsync(It.IsAny<ExchangeTokenRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BadRequestObjectResult("bad"));
        var controller = new ExchangeTokenController(_service.Object);
        var result = await controller.ExchangeToken(new ExchangeTokenRequest());
        result.Result.ShouldBeOfType<BadRequestObjectResult>();
    }
}
