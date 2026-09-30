using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.SystemSettings.GetSiteModelSettings.Domain;
using PolyBucket.Api.Features.SystemSettings.Http;
using PolyBucket.Tests.Testing;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.SystemSettings.Http;

public class GetSiteModelSettingsControllerTests
{
    private readonly Mock<IMediator> _mediator = new();

    [Fact(DisplayName = "When site model settings load successfully, get site model settings returns Ok.")]
    public async Task GetSiteModelSettings_Success_ReturnsOk()
    {
        // Arrange
        var response = new GetSiteModelSettingsResponse { Success = true };
        _mediator.SetupSend<GetSiteModelSettingsQuery, GetSiteModelSettingsResponse>(response);
        var controller = new GetSiteModelSettingsController(_mediator.Object);

        // Act
        var result = await controller.GetSiteModelSettings();

        // Assert
        result.Result.ShouldBeOfType<OkObjectResult>().Value.ShouldBe(response);
    }
}
