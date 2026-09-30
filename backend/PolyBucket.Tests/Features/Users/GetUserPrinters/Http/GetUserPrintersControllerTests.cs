using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using PolyBucket.Api.Features.Users.GetUserPrinters.Domain;
using PolyBucket.Api.Features.Users.GetUserPrinters.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Users.GetUserPrinters.Http;

public class GetUserPrintersControllerTests
{
    private readonly Mock<IGetUserPrintersService> _service = new();
    private readonly GetUserPrintersController _controller;

    public GetUserPrintersControllerTests()
    {
        _controller = new GetUserPrintersController(_service.Object, Mock.Of<ILogger<GetUserPrintersController>>());
    }

    [Fact(DisplayName = "When page is invalid, GetUserPrinters returns BadRequest.")]
    public async Task GetUserPrinters_InvalidPage_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.GetUserPrinters(Guid.NewGuid(), page: 0);

        // Assert
        result.Result.ShouldBeOfType<BadRequestObjectResult>();
    }

    [Fact(DisplayName = "When the service succeeds, GetUserPrinters returns Ok.")]
    public async Task GetUserPrinters_Success_ReturnsOk()
    {
        // Arrange
        var payload = new GetUserPrintersResult();
        _service.Setup(s => s.GetUserPrintersAsync(It.IsAny<GetUserPrintersQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(payload);

        // Act
        var result = await _controller.GetUserPrinters(Guid.NewGuid());

        // Assert
        result.Result.ShouldBeOfType<OkObjectResult>().Value.ShouldBe(payload);
    }
}
