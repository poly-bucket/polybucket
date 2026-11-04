using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.SystemSettings.BulkUpdateFileSettings.Domain;
using PolyBucket.Api.Features.SystemSettings.BulkUpdateFileSettings.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.SystemSettings.BulkUpdateFileSettings;

public class BulkUpdateFileSettingsControllerTests
{
    [Fact(DisplayName = "When ids are provided, bulk update returns Ok.")]
    public async Task BulkUpdate_WithIds_ReturnsOk()
    {
        // Arrange
        var response = new BulkUpdateFileSettingsResponse { Success = true, UpdatedCount = 1 };
        var service = new Mock<IBulkUpdateFileSettingsService>();
        service
            .Setup(s => s.BulkUpdateAsync(It.IsAny<BulkUpdateFileSettingsCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);
        var controller = new BulkUpdateFileSettingsController(service.Object);
        var command = new BulkUpdateFileSettingsCommand
        {
            Ids = new List<Guid> { Guid.NewGuid() },
            Enabled = true
        };

        // Act
        var result = await controller.BulkUpdateFileSettings(command, CancellationToken.None);

        // Assert
        result.Result.ShouldBeOfType<OkObjectResult>().Value.ShouldBe(response);
    }

    [Fact(DisplayName = "When no ids are provided, bulk update returns BadRequest.")]
    public async Task BulkUpdate_EmptyIds_ReturnsBadRequest()
    {
        // Arrange
        var service = new Mock<IBulkUpdateFileSettingsService>();
        var controller = new BulkUpdateFileSettingsController(service.Object);

        // Act
        var result = await controller.BulkUpdateFileSettings(
            new BulkUpdateFileSettingsCommand { Ids = new List<Guid>() },
            CancellationToken.None);

        // Assert
        result.Result.ShouldBeOfType<BadRequestObjectResult>();
    }
}
