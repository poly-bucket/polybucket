using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.SystemSettings.Http;
using PolyBucket.Api.Features.SystemSettings.UpdateModelConfigurationSettings.Domain;
using PolyBucket.Tests.Testing;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.SystemSettings.Http;

public class UpdateModelSettingsControllerTests
{
    private readonly Mock<IMediator> _mediator = new();

    [Fact(DisplayName = "When model configuration update succeeds, UpdateModelConfigurationSettings returns Ok.")]
    public async Task UpdateModelConfigurationSettings_Success_ReturnsOk()
    {
        // Arrange
        var response = new UpdateModelConfigurationSettingsResponse { Success = true, Message = "Updated" };
        _mediator.SetupSend<UpdateModelConfigurationSettingsCommand, UpdateModelConfigurationSettingsResponse>(response);
        var controller = new UpdateModelConfigurationSettingsController(_mediator.Object);

        // Act
        var result = await controller.UpdateModelConfigurationSettings(ValidCommand());

        // Assert
        result.Result.ShouldBeOfType<OkObjectResult>().Value.ShouldBe(response);
    }

    [Fact(DisplayName = "When model configuration update fails, UpdateModelConfigurationSettings returns BadRequest.")]
    public async Task UpdateModelConfigurationSettings_Failure_ReturnsBadRequest()
    {
        // Arrange
        var response = new UpdateModelConfigurationSettingsResponse { Success = false, Message = "Failed" };
        _mediator.SetupSend<UpdateModelConfigurationSettingsCommand, UpdateModelConfigurationSettingsResponse>(response);
        var controller = new UpdateModelConfigurationSettingsController(_mediator.Object);

        // Act
        var result = await controller.UpdateModelConfigurationSettings(ValidCommand());

        // Assert
        result.Result.ShouldBeOfType<BadRequestObjectResult>().Value.ShouldBe(response);
    }

    private static UpdateModelConfigurationSettingsCommand ValidCommand() => new()
    {
        DefaultPrivacySetting = "Public",
        MinDescriptionLength = 10,
        MaxDescriptionLength = 2000
    };
}
