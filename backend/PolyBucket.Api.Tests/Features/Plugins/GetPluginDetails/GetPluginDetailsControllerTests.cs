using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Plugins.GetPluginDetails.Domain;
using PolyBucket.Api.Features.Plugins.GetPluginDetails.Http;
using PolyBucket.Api.Features.Plugins.GetPluginsOverview.Domain;
using PolyBucket.Api.Features.Plugins.GetPluginsOverview.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Plugins.GetPluginDetails;

public class GetPluginDetailsControllerTests
{
    [Fact(DisplayName = "When the plugin id is unknown, get plugin details returns NotFound.")]
    public void GetPluginDetails_Missing_ReturnsNotFound()
    {
        // Arrange
        var service = new Mock<IGetPluginDetailsService>();
        service.Setup(s => s.GetDetails("missing")).Returns((GetPluginDetailsResponse?)null);
        var controller = new GetPluginDetailsController(service.Object);

        // Act
        var result = controller.GetPluginDetails("missing");

        // Assert
        result.Result.ShouldBeOfType<NotFoundObjectResult>();
    }

    [Fact(DisplayName = "When the plugin is loaded, get plugin details returns Ok.")]
    public void GetPluginDetails_Found_ReturnsOk()
    {
        // Arrange
        var service = new Mock<IGetPluginDetailsService>();
        service.Setup(s => s.GetDetails("meta")).Returns(new GetPluginDetailsResponse { Id = "meta", Name = "Metadata" });
        var controller = new GetPluginDetailsController(service.Object);

        // Act
        var result = controller.GetPluginDetails("meta");

        // Assert
        result.Result.ShouldBeOfType<OkObjectResult>();
    }
}
