using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Plugins.GetPlugins.Domain;
using PolyBucket.Api.Features.Plugins.GetPlugins.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Plugins.GetPlugins;

public class GetPluginsControllerTests
{
    [Fact(DisplayName = "When plugins are loaded, get plugins returns Ok with plugin info.")]
    public void GetPlugins_ReturnsOk()
    {
        // Arrange
        var service = new Mock<IGetPluginsService>();
        service.Setup(s => s.GetPlugins()).Returns(new List<GetPluginsItem>
        {
            new() { Id = "comments", Name = "Comments", Version = "1.0.0", Author = "PolyBucket", Description = "Comments plugin" }
        });
        var controller = new GetPluginsController(service.Object);

        // Act
        var result = controller.GetPlugins();

        // Assert
        var ok = result.Result.ShouldBeOfType<OkObjectResult>();
        var items = ok.Value.ShouldBeAssignableTo<IEnumerable<GetPluginsResponseDto>>();
        items.Single().Id.ShouldBe("comments");
    }
}
