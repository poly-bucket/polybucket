using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.SystemSettings.GetActiveExtensibleTheme.Domain;
using PolyBucket.Api.Features.SystemSettings.GetActiveExtensibleTheme.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.SystemSettings.GetActiveExtensibleTheme;

public class GetActiveExtensibleThemeControllerTests
{
    [Fact(DisplayName = "When no active theme is set, GetActiveTheme returns NotFound.")]
    public async Task GetActiveTheme_Missing_ReturnsNotFound()
    {
        // Arrange
        var service = new Mock<IGetActiveExtensibleThemeService>();
        service.Setup(s => s.GetActiveExtensibleThemeAsync(It.IsAny<CancellationToken>())).ReturnsAsync((PolyBucket.Api.Common.Plugins.ThemeDefinition?)null);
        var controller = new GetActiveExtensibleThemeController(service.Object);

        // Act
        var result = await controller.GetActiveTheme(CancellationToken.None);

        // Assert
        result.Result.ShouldBeOfType<NotFoundObjectResult>();
    }
}
