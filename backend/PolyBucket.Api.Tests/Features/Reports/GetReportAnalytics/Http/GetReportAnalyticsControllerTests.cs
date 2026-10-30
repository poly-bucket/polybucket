using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Reports.GetReportsAnalytics.Domain;
using PolyBucket.Api.Features.Reports.GetReportsAnalytics.Http;
using PolyBucket.Api.Features.Reports.GetReportTrends.Domain;
using PolyBucket.Api.Features.Reports.GetReportTrends.Http;
using PolyBucket.Api.Features.Reports.GetTopReportedModels.Domain;
using PolyBucket.Api.Features.Reports.GetTopReportedModels.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Reports.GetReportAnalytics.Http;

public class GetReportAnalyticsControllerTests
{
    [Fact(DisplayName = "When from date is after to date, analytics returns BadRequest.")]
    public async Task GetReportsAnalytics_InvalidRange_ReturnsBadRequest()
    {
        // Arrange
        var service = new Mock<IGetReportsAnalyticsService>();
        var controller = new GetReportsAnalyticsController(service.Object);
        var from = DateTime.UtcNow;
        var to = from.AddDays(-1);

        // Act
        var result = await controller.GetReportsAnalytics(from, to, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<BadRequestObjectResult>();
        service.VerifyNoOtherCalls();
    }

    [Fact(DisplayName = "When the date range is valid, analytics returns Ok.")]
    public async Task GetReportsAnalytics_Valid_ReturnsOk()
    {
        // Arrange
        var service = new Mock<IGetReportsAnalyticsService>();
        var analytics = new ReportsAnalyticsResponseDto();
        service.Setup(s => s.GetReportsAnalyticsAsync(null, null, It.IsAny<CancellationToken>())).ReturnsAsync(analytics);
        var controller = new GetReportsAnalyticsController(service.Object);

        // Act
        var result = await controller.GetReportsAnalytics(cancellationToken: CancellationToken.None);

        // Assert
        result.ShouldBeOfType<OkObjectResult>().Value.ShouldBe(analytics);
    }

    [Theory(DisplayName = "Invalid top-reported limit returns BadRequest.")]
    [InlineData(0)]
    [InlineData(101)]
    public async Task GetTopReportedModels_InvalidLimit_ReturnsBadRequest(int limit)
    {
        // Arrange
        var service = new Mock<IGetTopReportedModelsService>();
        var controller = new GetTopReportedModelsController(service.Object);

        // Act
        var result = await controller.GetTopReportedModels(limit, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<BadRequestObjectResult>();
    }

    [Fact(DisplayName = "Invalid trends period returns BadRequest.")]
    public async Task GetReportTrends_InvalidPeriod_ReturnsBadRequest()
    {
        // Arrange
        var service = new Mock<IGetReportTrendsService>();
        var controller = new GetReportTrendsController(service.Object);

        // Act
        var result = await controller.GetReportTrends("yearly", 30, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<BadRequestObjectResult>();
    }

    [Fact(DisplayName = "Valid trends request returns Ok.")]
    public async Task GetReportTrends_Valid_ReturnsOk()
    {
        // Arrange
        var service = new Mock<IGetReportTrendsService>();
        var trends = new ReportTrendsResponseDto();
        service.Setup(s => s.GetReportTrendsAsync("daily", 30, It.IsAny<CancellationToken>())).ReturnsAsync(trends);
        var controller = new GetReportTrendsController(service.Object);

        // Act
        var result = await controller.GetReportTrends("daily", 30, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<OkObjectResult>().Value.ShouldBe(trends);
    }
}
