using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.SystemSettings.SyncAllowedFileTypes.Domain;
using PolyBucket.Api.Features.SystemSettings.SyncAllowedFileTypes.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.SystemSettings.SyncAllowedFileTypes;

public class SyncAllowedFileTypesControllerTests
{
    [Fact(DisplayName = "When sync succeeds, sync allowed types returns Ok.")]
    public async Task SyncAllowedFileTypes_Success_ReturnsOk()
    {
        // Arrange
        var response = new SyncAllowedFileTypesResponse
        {
            Success = true,
            DerivedAllowedFileTypes = ".stl,.obj"
        };
        var service = new Mock<ISyncAllowedFileTypesService>();
        service.Setup(s => s.SyncAsync(It.IsAny<CancellationToken>())).ReturnsAsync(response);
        var controller = new SyncAllowedFileTypesController(service.Object);

        // Act
        var result = await controller.SyncAllowedFileTypes(CancellationToken.None);

        // Assert
        result.Result.ShouldBeOfType<OkObjectResult>().Value.ShouldBe(response);
    }

    [Fact(DisplayName = "When sync fails, sync allowed types returns BadRequest.")]
    public async Task SyncAllowedFileTypes_Failure_ReturnsBadRequest()
    {
        // Arrange
        var response = new SyncAllowedFileTypesResponse { Success = false };
        var service = new Mock<ISyncAllowedFileTypesService>();
        service.Setup(s => s.SyncAsync(It.IsAny<CancellationToken>())).ReturnsAsync(response);
        var controller = new SyncAllowedFileTypesController(service.Object);

        // Act
        var result = await controller.SyncAllowedFileTypes(CancellationToken.None);

        // Assert
        result.Result.ShouldBeOfType<BadRequestObjectResult>().Value.ShouldBe(response);
    }
}
