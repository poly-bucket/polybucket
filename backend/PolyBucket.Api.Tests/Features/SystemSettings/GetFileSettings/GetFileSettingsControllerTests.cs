using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.SystemSettings.GetFileSettings.Domain;
using PolyBucket.Api.Features.SystemSettings.GetFileSettings.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.SystemSettings.GetFileSettings;

public class GetFileSettingsControllerTests
{
    [Fact(DisplayName = "When file settings load successfully, get file settings returns Ok.")]
    public async Task GetFileSettings_Success_ReturnsOk()
    {
        // Arrange
        var response = new GetFileSettingsResponse { Success = true };
        var service = new Mock<IGetFileSettingsService>();
        service
            .Setup(s => s.GetFileSettingsAsync(It.IsAny<GetFileSettingsQueryParams>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);
        var controller = new GetFileSettingsController(service.Object);

        // Act
        var result = await controller.GetFileSettings(cancellationToken: CancellationToken.None);

        // Assert
        result.Result.ShouldBeOfType<OkObjectResult>().Value.ShouldBe(response);
    }

    [Fact(DisplayName = "When page is less than 1, get file settings returns BadRequest.")]
    public async Task GetFileSettings_InvalidPage_ReturnsBadRequest()
    {
        // Arrange
        var service = new Mock<IGetFileSettingsService>();
        var controller = new GetFileSettingsController(service.Object);

        // Act
        var result = await controller.GetFileSettings(page: 0, cancellationToken: CancellationToken.None);

        // Assert
        result.Result.ShouldBeOfType<BadRequestObjectResult>();
    }

    [Fact(DisplayName = "When page size is out of range, get file settings returns BadRequest.")]
    public async Task GetFileSettings_InvalidPageSize_ReturnsBadRequest()
    {
        // Arrange
        var service = new Mock<IGetFileSettingsService>();
        var controller = new GetFileSettingsController(service.Object);

        // Act
        var result = await controller.GetFileSettings(pageSize: 101, cancellationToken: CancellationToken.None);

        // Assert
        result.Result.ShouldBeOfType<BadRequestObjectResult>();
    }
}
