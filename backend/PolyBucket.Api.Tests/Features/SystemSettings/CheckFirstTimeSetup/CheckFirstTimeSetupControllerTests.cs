using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.SystemSettings.CheckFirstTimeSetup.Domain;
using PolyBucket.Api.Features.SystemSettings.CheckFirstTimeSetup.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.SystemSettings.CheckFirstTimeSetup;

public class CheckFirstTimeSetupControllerTests
{
    [Fact(DisplayName = "Get setup status returns Ok with the service response.")]
    public async Task GetSetupStatus_ReturnsOk()
    {
        // Arrange
        var response = new CheckFirstTimeSetupResponse { IsFirstTimeSetup = true };
        var service = new Mock<ICheckFirstTimeSetupService>();
        service.Setup(s => s.CheckFirstTimeSetupAsync(It.IsAny<CheckFirstTimeSetupQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(response);
        var controller = new CheckFirstTimeSetupController(service.Object);

        // Act
        var result = await controller.GetSetupStatus(CancellationToken.None);

        // Assert
        result.Result.ShouldBeOfType<OkObjectResult>().Value.ShouldBe(response);
    }
}
