using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Reports.Domain;
using PolyBucket.Api.Features.Reports.GetReport.Domain;
using PolyBucket.Api.Features.Reports.GetReport.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Reports.GetReport.Http;

public class GetReportControllerTests
{
    private readonly Mock<IGetReportService> _service = new();

    [Fact(DisplayName = "When the report exists, get report returns Ok.")]
    public async Task GetReport_Found_ReturnsOk()
    {
        // Arrange
        var reportId = Guid.NewGuid();
        var report = new Report
        {
            Type = ReportType.Model,
            TargetId = Guid.NewGuid(),
            ReporterId = Guid.NewGuid(),
            Reason = ReportReason.Spam,
            Description = "spam",
            Resolution = string.Empty
        };
        _service.Setup(s => s.GetReportByIdAsync(reportId, It.IsAny<CancellationToken>())).ReturnsAsync(report);
        var controller = new GetReportController(_service.Object);

        // Act
        var result = await controller.GetReport(reportId, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<OkObjectResult>().Value.ShouldBe(report);
    }

    [Fact(DisplayName = "When the report is missing, get report returns NotFound.")]
    public async Task GetReport_Missing_ReturnsNotFound()
    {
        // Arrange
        _service.Setup(s => s.GetReportByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Report?)null);
        var controller = new GetReportController(_service.Object);

        // Act
        var result = await controller.GetReport(Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.ShouldBeOfType<NotFoundResult>();
    }
}
