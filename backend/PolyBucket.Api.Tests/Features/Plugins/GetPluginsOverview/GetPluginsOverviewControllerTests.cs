using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Plugins.GetPluginsOverview.Domain;
using PolyBucket.Api.Features.Plugins.GetPluginsOverview.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Plugins.GetPluginsOverview;

public class GetPluginsOverviewControllerTests
{
    [Fact(DisplayName = "Get plugins overview returns counts for loaded plugins.")]
    public void GetPluginsOverview_ReturnsOk()
    {
        // Arrange
        var service = new Mock<IGetPluginsOverviewService>();
        service.Setup(s => s.GetOverview()).Returns(new GetPluginsOverviewResponse { TotalPlugins = 1, ActivePlugins = 1 });
        var controller = new GetPluginsOverviewController(service.Object);

        // Act
        var result = controller.GetPluginsOverview();

        // Assert
        var ok = result.Result.ShouldBeOfType<OkObjectResult>();
        var body = ok.Value.ShouldBeOfType<GetPluginsOverviewResponse>();
        body.TotalPlugins.ShouldBe(1);
    }
}
