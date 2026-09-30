using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using PolyBucket.Api.Features.Admin.GetAdminModelStatistics.Domain;
using PolyBucket.Api.Features.Admin.GetAdminModelStatistics.Http;
using PolyBucket.Tests.Testing;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Admin.GetAdminModelStatistics;

public class GetAdminModelStatisticsControllerTests
{
    private readonly Mock<IGetAdminModelStatisticsService> _service = new();
    private readonly GetAdminModelStatisticsController _controller;

    public GetAdminModelStatisticsControllerTests()
    {
        _controller = new GetAdminModelStatisticsController(
            _service.Object,
            Mock.Of<ILogger<GetAdminModelStatisticsController>>()).WithUser(Guid.NewGuid());
    }

    [Fact(DisplayName = "When statistics are available, GetAdminModelStatistics returns Ok with the response.")]
    public async Task GetAdminModelStatistics_Success_ReturnsOk()
    {
        // Arrange
        var stats = new GetAdminModelStatisticsResponse { TotalModels = 42, PublicModels = 30 };
        _service.Setup(s => s.GetAdminModelStatisticsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(stats);

        // Act
        var result = await _controller.GetAdminModelStatistics(CancellationToken.None);

        // Assert
        result.Result.ShouldBeOfType<OkObjectResult>().Value.ShouldBe(stats);
    }

    [Fact(DisplayName = "When the service throws, GetAdminModelStatistics returns 500.")]
    public async Task GetAdminModelStatistics_ServiceError_Returns500()
    {
        // Arrange
        _service.Setup(s => s.GetAdminModelStatisticsAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("db down"));

        // Act
        var result = await _controller.GetAdminModelStatistics(CancellationToken.None);

        // Assert
        var error = result.Result.ShouldBeOfType<ObjectResult>();
        error.StatusCode.ShouldBe(500);
    }
}
