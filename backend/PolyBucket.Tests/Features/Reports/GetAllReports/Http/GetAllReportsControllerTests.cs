using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Reports.Domain;
using PolyBucket.Api.Features.Reports.GetAllReports.Domain;
using PolyBucket.Api.Features.Reports.GetAllReports.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Reports.GetAllReports.Http;

public class GetAllReportsControllerTests
{
    private readonly Mock<IGetAllReportsService> _service = new();

    [Fact(DisplayName = "Valid pagination returns reports.")]
    public async Task GetAllReports_Valid_ReturnsOk()
    {
        // Arrange
        var response = new ReportsResponse();
        _service.Setup(s => s.GetAllReportsAsync(1, 20, null, null, It.IsAny<CancellationToken>())).ReturnsAsync(response);
        var controller = new GetAllReportsController(_service.Object);

        // Act
        var result = await controller.GetAllReports(1, 20, null, null, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<OkObjectResult>().Value.ShouldBe(response);
    }

    [Theory(DisplayName = "Invalid pagination returns BadRequest.")]
    [InlineData(0, 20)]
    [InlineData(1, 0)]
    [InlineData(1, 101)]
    public async Task GetAllReports_InvalidPaging_ReturnsBadRequest(int page, int pageSize)
    {
        // Arrange
        var controller = new GetAllReportsController(_service.Object);

        // Act
        var result = await controller.GetAllReports(page, pageSize, null, null, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<BadRequestObjectResult>();
        _service.VerifyNoOtherCalls();
    }
}
