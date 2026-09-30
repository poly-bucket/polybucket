using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.SystemSettings.GetFileSettings.Domain;
using PolyBucket.Api.Features.SystemSettings.Http;
using PolyBucket.Tests.Testing;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.SystemSettings.Http;

public class GetFileSettingsControllerTests
{
    private readonly Mock<IMediator> _mediator = new();

    [Fact(DisplayName = "When file settings load successfully, get file settings returns Ok.")]
    public async Task GetFileSettings_Success_ReturnsOk()
    {
        // Arrange
        var response = new GetFileSettingsResponse { Success = true };
        _mediator.SetupSend<GetFileSettingsQuery, GetFileSettingsResponse>(response);
        var controller = new GetFileSettingsController(_mediator.Object);

        // Act
        var result = await controller.GetFileSettings();

        // Assert
        result.Result.ShouldBeOfType<OkObjectResult>().Value.ShouldBe(response);
    }
}
