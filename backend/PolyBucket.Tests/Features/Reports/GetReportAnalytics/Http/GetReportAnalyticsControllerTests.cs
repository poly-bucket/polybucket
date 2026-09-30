using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Reports.Domain;
using PolyBucket.Api.Features.Reports.GetReportAnalytics.Domain;
using PolyBucket.Api.Features.Reports.GetReportAnalytics.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Reports.GetReportAnalytics.Http;

public class GetReportAnalyticsControllerTests
{
    private readonly Mock<IGetReportAnalyticsService> _service = new();

    [Fact(DisplayName = "When from date is after to date, analytics returns BadRequest.")]
    public async Task GetReportsAnalytics_InvalidRange_ReturnsBadRequest()
    {
        // Arrange
        var controller = new GetReportAnalyticsController(_service.Object);
        var from = DateTime.UtcNow;
        var to = from.AddDays(-1);

        // Act
        var result = await controller.GetReportsAnalytics(from, to, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<BadRequestObjectResult>();
        _service.VerifyNoOtherCalls();
    }

    [Fact(DisplayName = "When the date range is valid, analytics returns Ok.")]
    public async Task GetReportsAnalytics_Valid_ReturnsOk()
    {
        // Arrange
        var analytics = new ReportsAnalytics();
        _service.Setup(s => s.GetReportsAnalyticsAsync(null, null, It.IsAny<CancellationToken>())).ReturnsAsync(analytics);
        var controller = new GetReportAnalyticsController(_service.Object);

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
        var controller = new GetReportAnalyticsController(_service.Object);

        // Act
        var result = await controller.GetTopReportedModels(limit, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<BadRequestObjectResult>();
    }

    [Fact(DisplayName = "Invalid trends period returns BadRequest.")]
    public async Task GetReportTrends_InvalidPeriod_ReturnsBadRequest()
    {
        // Arrange
        var controller = new GetReportAnalyticsController(_service.Object);

        // Act
        var result = await controller.GetReportTrends("yearly", 30, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<BadRequestObjectResult>();
    }

    [Fact(DisplayName = "Valid trends request returns Ok.")]
    public async Task GetReportTrends_Valid_ReturnsOk()
    {
        // Arrange
        var trends = new List<ReportTrend>();
        _service.Setup(s => s.GetReportTrendsAsync("daily", 30, It.IsAny<CancellationToken>())).ReturnsAsync(trends);
        var controller = new GetReportAnalyticsController(_service.Object);

        // Act
        var result = await controller.GetReportTrends("daily", 30, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<OkObjectResult>().Value.ShouldBe(trends);
    }
}
