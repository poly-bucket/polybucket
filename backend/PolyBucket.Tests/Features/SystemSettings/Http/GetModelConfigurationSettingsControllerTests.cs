using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.SystemSettings.GetModelConfigurationSettings.Domain;
using PolyBucket.Api.Features.SystemSettings.Http;
using PolyBucket.Tests.Testing;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.SystemSettings.Http;

public class GetModelConfigurationSettingsControllerTests
{
    private readonly Mock<IMediator> _mediator = new();

    [Fact(DisplayName = "When settings load successfully, get model configuration returns Ok.")]
    public async Task GetModelConfigurationSettings_Success_ReturnsOk()
    {
        // Arrange
        var response = new GetModelConfigurationSettingsResponse { Success = true, Settings = new ModelConfigurationSettingsData() };
        _mediator.SetupSend<GetModelConfigurationSettingsQuery, GetModelConfigurationSettingsResponse>(response);
        var controller = new GetModelConfigurationSettingsController(_mediator.Object);

        // Act
        var result = await controller.GetModelConfigurationSettings();

        // Assert
        result.Result.ShouldBeOfType<OkObjectResult>().Value.ShouldBe(response);
    }

    [Fact(DisplayName = "When settings fail to load, get model configuration returns BadRequest.")]
    public async Task GetModelConfigurationSettings_Failure_ReturnsBadRequest()
    {
        // Arrange
        var response = new GetModelConfigurationSettingsResponse { Success = false, Message = "error" };
        _mediator.SetupSend<GetModelConfigurationSettingsQuery, GetModelConfigurationSettingsResponse>(response);
        var controller = new GetModelConfigurationSettingsController(_mediator.Object);

        // Act
        var result = await controller.GetModelConfigurationSettings();

        // Assert
        result.Result.ShouldBeOfType<BadRequestObjectResult>().Value.ShouldBe(response);
    }
}
