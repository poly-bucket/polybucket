using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Common.Plugins;
using PolyBucket.Api.Features.SystemSettings.GetExtensibleThemes.Domain;
using PolyBucket.Api.Features.SystemSettings.GetExtensibleThemes.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.SystemSettings.GetExtensibleThemes;

public class GetExtensibleThemesControllerTests
{
    [Fact(DisplayName = "When themes exist, GetAvailableThemes returns Ok.")]
    public async Task GetAvailableThemes_ReturnsOk()
    {
        // Arrange
        var service = new Mock<IGetExtensibleThemesService>();
        service.Setup(s => s.GetExtensibleThemesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<ThemeDefinition>());
        var controller = new GetExtensibleThemesController(service.Object);

        // Act
        var result = await controller.GetAvailableThemes(CancellationToken.None);

        // Assert
        result.Result.ShouldBeOfType<OkObjectResult>();
    }
}
