using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Reports.Domain;
using PolyBucket.Api.Features.Reports.GetUnresolvedReports.Domain;
using PolyBucket.Api.Features.Reports.GetUnresolvedReports.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Reports.GetUnresolvedReports.Http;

public class GetUnresolvedReportsControllerTests
{
    [Fact(DisplayName = "Get unresolved reports returns Ok with the service list.")]
    public async Task GetUnresolvedReports_ReturnsOk()
    {
        // Arrange
        var service = new Mock<IGetUnresolvedReportsService>();
        var reports = new List<Report>();
        service.Setup(s => s.GetUnresolvedReportsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(reports);
        var controller = new GetUnresolvedReportsController(service.Object);

        // Act
        var result = await controller.GetUnresolvedReports(CancellationToken.None);

        // Assert
        result.ShouldBeOfType<OkObjectResult>().Value.ShouldBe(reports);
    }
}
