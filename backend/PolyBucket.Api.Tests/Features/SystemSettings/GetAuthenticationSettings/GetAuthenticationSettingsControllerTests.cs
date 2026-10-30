using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.SystemSettings.Domain;
using PolyBucket.Api.Features.SystemSettings.GetAuthenticationSettings.Domain;
using PolyBucket.Api.Features.SystemSettings.GetAuthenticationSettings.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.SystemSettings.GetAuthenticationSettings;

public class GetAuthenticationSettingsControllerTests
{
    [Fact(DisplayName = "The get authentication settings controller requires the Admin role.")]
    public void Controller_ShouldRequireAdminRole()
    {
        // Arrange
        var authorizeAttr = typeof(GetAuthenticationSettingsController).GetCustomAttribute<AuthorizeAttribute>();

        // Act & Assert
        authorizeAttr.ShouldNotBeNull();
        authorizeAttr!.Roles.ShouldBe("Admin");
    }

    [Fact(DisplayName = "When authentication settings load, GetAuthenticationSettings returns Ok.")]
    public async Task GetAuthenticationSettings_Success_ReturnsOk()
    {
        // Arrange
        var settings = new AuthenticationSettings { AllowEmailLogin = true };
        var service = new Mock<IGetAuthenticationSettingsService>();
        service.Setup(s => s.GetAuthenticationSettingsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(settings);
        var controller = new GetAuthenticationSettingsController(service.Object);

        // Act
        var result = await controller.GetAuthenticationSettings(CancellationToken.None);

        // Assert
        result.Result.ShouldBeOfType<OkObjectResult>().Value.ShouldBe(settings);
    }
}
