using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Reports.ResolveReport.Domain;
using PolyBucket.Api.Features.Reports.ResolveReport.Http;
using PolyBucket.Tests.Testing;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Reports.ResolveReport.Http;

public class ResolveReportControllerTests
{
    private readonly Mock<IResolveReportService> _service = new();
    private readonly Guid _moderatorId = Guid.NewGuid();

    [Fact(DisplayName = "When resolve succeeds, the controller returns Ok.")]
    public async Task ResolveReport_Valid_ReturnsOk()
    {
        // Arrange
        var reportId = Guid.NewGuid();
        _service
            .Setup(s => s.ResolveReportAsync(reportId, _moderatorId, "dismissed", It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var controller = new ResolveReportController(_service.Object).WithUser(_moderatorId);

        // Act
        var result = await controller.ResolveReport(reportId, new ResolveReportRequest { Resolution = "dismissed" }, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<OkResult>();
    }
}
