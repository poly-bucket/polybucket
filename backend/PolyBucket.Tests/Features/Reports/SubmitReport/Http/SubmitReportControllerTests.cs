using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Reports.Domain;
using PolyBucket.Api.Features.Reports.SubmitReport.Domain;
using PolyBucket.Api.Features.Reports.SubmitReport.Http;
using PolyBucket.Tests.Testing;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Reports.SubmitReport.Http;

public class SubmitReportControllerTests
{
    private readonly Mock<ISubmitReportService> _service = new();
    private readonly Guid _userId = Guid.NewGuid();

    [Fact(DisplayName = "When submit succeeds, the controller returns Ok with the created report.")]
    public async Task SubmitReport_Valid_ReturnsOk()
    {
        // Arrange
        var request = new SubmitReportRequest
        {
            Type = ReportType.Model,
            TargetId = Guid.NewGuid(),
            Reason = ReportReason.Spam,
            Description = "Looks like spam content"
        };
        var report = new Report
        {
            Type = request.Type,
            TargetId = request.TargetId,
            ReporterId = _userId,
            Reason = request.Reason,
            Description = request.Description,
            Resolution = string.Empty
        };
        _service
            .Setup(s => s.SubmitReportAsync(request.Type, request.TargetId, _userId, request.Reason, request.Description, It.IsAny<CancellationToken>()))
            .ReturnsAsync(report);
        var controller = new SubmitReportController(_service.Object).WithUser(_userId);

        // Act
        var result = await controller.SubmitReport(request, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<OkObjectResult>().Value.ShouldBe(report);
    }
}
