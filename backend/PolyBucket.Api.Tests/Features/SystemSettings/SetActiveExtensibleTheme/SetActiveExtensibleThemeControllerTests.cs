using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.SystemSettings.SetActiveExtensibleTheme.Domain;
using PolyBucket.Api.Features.SystemSettings.SetActiveExtensibleTheme.Http;
using PolyBucket.Api.Tests.Testing;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.SystemSettings.SetActiveExtensibleTheme;

public class SetActiveExtensibleThemeControllerTests
{
    [Fact(DisplayName = "When set active theme fails, SetActiveTheme returns BadRequest.")]
    public async Task SetActiveTheme_Failure_ReturnsBadRequest()
    {
        // Arrange
        var service = new Mock<ISetActiveExtensibleThemeService>();
        service.Setup(s => s.SetActiveExtensibleThemeAsync("dark", It.IsAny<CancellationToken>())).ReturnsAsync(false);
        var controller = new SetActiveExtensibleThemeController(service.Object).WithUser(Guid.NewGuid(), "Admin");

        // Act
        var result = await controller.SetActiveTheme("dark", CancellationToken.None);

        // Assert
        result.ShouldBeOfType<BadRequestObjectResult>();
    }
}
