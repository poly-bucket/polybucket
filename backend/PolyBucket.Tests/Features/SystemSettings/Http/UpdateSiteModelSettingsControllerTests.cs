using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.SystemSettings.Http;
using PolyBucket.Api.Features.SystemSettings.UpdateSiteModelSettings.Domain;
using PolyBucket.Tests.Testing;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.SystemSettings.Http;

public class UpdateSiteModelSettingsControllerTests
{
    private readonly Mock<IMediator> _mediator = new();

    [Fact(DisplayName = "When site model settings update succeeds, UpdateSiteModelSettings returns Ok.")]
    public async Task UpdateSiteModelSettings_Success_ReturnsOk()
    {
        // Arrange
        var response = new UpdateSiteModelSettingsResponse { Success = true, Message = "Updated" };
        _mediator.SetupSend<UpdateSiteModelSettingsCommand, UpdateSiteModelSettingsResponse>(response);
        var controller = new UpdateSiteModelSettingsController(_mediator.Object);

        // Act
        var result = await controller.UpdateSiteModelSettings(ValidCommand());

        // Assert
        result.Result.ShouldBeOfType<OkObjectResult>().Value.ShouldBe(response);
    }

    [Fact(DisplayName = "When site model settings update fails, UpdateSiteModelSettings returns BadRequest.")]
    public async Task UpdateSiteModelSettings_Failure_ReturnsBadRequest()
    {
        // Arrange
        var response = new UpdateSiteModelSettingsResponse { Success = false, Message = "Failed" };
        _mediator.SetupSend<UpdateSiteModelSettingsCommand, UpdateSiteModelSettingsResponse>(response);
        var controller = new UpdateSiteModelSettingsController(_mediator.Object);

        // Act
        var result = await controller.UpdateSiteModelSettings(ValidCommand());

        // Assert
        result.Result.ShouldBeOfType<BadRequestObjectResult>().Value.ShouldBe(response);
    }

    private static UpdateSiteModelSettingsCommand ValidCommand() => new()
    {
        DefaultModelPrivacy = "Public",
        AllowedFileTypes = ".stl,.obj"
    };
}
