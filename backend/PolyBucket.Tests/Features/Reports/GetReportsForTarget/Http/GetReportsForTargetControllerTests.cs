using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Reports.Domain;
using PolyBucket.Api.Features.Reports.GetReportsForTarget.Domain;
using PolyBucket.Api.Features.Reports.GetReportsForTarget.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Reports.GetReportsForTarget.Http;

public class GetReportsForTargetControllerTests
{
    [Fact(DisplayName = "Get reports for target returns Ok with matching reports.")]
    public async Task GetReportsForTarget_ReturnsOk()
    {
        // Arrange
        var targetId = Guid.NewGuid();
        var reports = new List<Report>();
        var service = new Mock<IGetReportsForTargetService>();
        service.Setup(s => s.GetReportsForTargetAsync(ReportType.Model, targetId, It.IsAny<CancellationToken>())).ReturnsAsync(reports);
        var controller = new GetReportsForTargetController(service.Object);

        // Act
        var result = await controller.GetReportsForTarget(targetId, ReportType.Model, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<OkObjectResult>().Value.ShouldBe(reports);
    }
}
