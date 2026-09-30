using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.SystemSettings.Http;
using PolyBucket.Api.Features.SystemSettings.UpdateFileSettings.Domain;
using PolyBucket.Tests.Testing;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.SystemSettings.Http;

public class UpdateFileSettingsControllerTests
{
    private readonly Mock<IMediator> _mediator = new();

    [Fact(DisplayName = "When update succeeds, UpdateFileSettings returns Ok.")]
    public async Task UpdateFileSettings_Success_ReturnsOk()
    {
        // Arrange
        var response = new UpdateFileSettingsResponse { Success = true, Message = "Updated" };
        _mediator.SetupSend<UpdateFileSettingsCommand, UpdateFileSettingsResponse>(response);
        var controller = new UpdateFileSettingsController(_mediator.Object);
        var command = ValidCommand();

        // Act
        var result = await controller.UpdateFileSettings(command);

        // Assert
        result.Result.ShouldBeOfType<OkObjectResult>().Value.ShouldBe(response);
    }

    [Fact(DisplayName = "When update fails, UpdateFileSettings returns BadRequest.")]
    public async Task UpdateFileSettings_Failure_ReturnsBadRequest()
    {
        // Arrange
        var response = new UpdateFileSettingsResponse { Success = false, Message = "Failed" };
        _mediator.SetupSend<UpdateFileSettingsCommand, UpdateFileSettingsResponse>(response);
        var controller = new UpdateFileSettingsController(_mediator.Object);

        // Act
        var result = await controller.UpdateFileSettings(ValidCommand());

        // Assert
        result.Result.ShouldBeOfType<BadRequestObjectResult>().Value.ShouldBe(response);
    }

    private static UpdateFileSettingsCommand ValidCommand() => new()
    {
        Id = Guid.NewGuid(),
        FileExtension = ".stl",
        Enabled = true,
        MaxFileSizeBytes = 1024,
        MaxPerUpload = 1,
        DisplayName = "STL",
        Description = "Stereolithography",
        MimeType = "model/stl",
        Category = "model",
        Priority = 1
    };
}
