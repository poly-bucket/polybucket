using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PolyBucket.Api.Features.SystemSettings.Domain;
using PolyBucket.Api.Features.SystemSettings.Http;
using PolyBucket.Api.Features.SystemSettings.Services;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.SystemSettings;

public class AuthenticationSettingsControllerTests
{
    private readonly Mock<IAuthenticationSettingsService> _service = new();

    [Fact(DisplayName = "The authentication settings controller is marked as an API controller with a route.")]
    public void Controller_ShouldHaveApiControllerAndRoute()
    {
        // Arrange
        var controllerType = typeof(AuthenticationSettingsController);

        // Act
        var apiAttr = controllerType.GetCustomAttribute<ApiControllerAttribute>();
        var routeAttr = controllerType.GetCustomAttribute<RouteAttribute>();

        // Assert
        apiAttr.ShouldNotBeNull();
        routeAttr.ShouldNotBeNull();
    }

    [Fact(DisplayName = "The authentication settings controller requires the Admin role.")]
    public void Controller_ShouldRequireAdminRole()
    {
        // Arrange
        var controllerType = typeof(AuthenticationSettingsController);

        // Act
        var authorizeAttr = controllerType.GetCustomAttribute<AuthorizeAttribute>();

        // Assert
        authorizeAttr.ShouldNotBeNull();
        authorizeAttr.Roles.ShouldBe("Admin");
    }

    [Fact(DisplayName = "When authentication settings load, GetAuthenticationSettings returns Ok.")]
    public async Task GetAuthenticationSettings_Success_ReturnsOk()
    {
        // Arrange
        var settings = new AuthenticationSettings { AllowEmailLogin = true, AllowUsernameLogin = false };
        _service.Setup(s => s.GetAuthenticationSettingsAsync()).ReturnsAsync(settings);
        var controller = new AuthenticationSettingsController(_service.Object, NullLogger<AuthenticationSettingsController>.Instance);

        // Act
        var result = await controller.GetAuthenticationSettings();

        // Assert
        result.Result.ShouldBeOfType<OkObjectResult>().Value.ShouldBe(settings);
    }

    [Fact(DisplayName = "When settings are invalid, UpdateAuthenticationSettings returns BadRequest.")]
    public async Task UpdateAuthenticationSettings_Invalid_ReturnsBadRequest()
    {
        // Arrange
        var controller = new AuthenticationSettingsController(_service.Object, NullLogger<AuthenticationSettingsController>.Instance);
        var invalid = new AuthenticationSettings { AllowEmailLogin = false, AllowUsernameLogin = false };

        // Act
        var result = await controller.UpdateAuthenticationSettings(invalid);

        // Assert
        result.Result.ShouldBeOfType<BadRequestObjectResult>();
        _service.Verify(s => s.UpdateAuthenticationSettingsAsync(It.IsAny<AuthenticationSettings>()), Times.Never);
    }

    [Fact(DisplayName = "When update succeeds, UpdateAuthenticationSettings returns Ok.")]
    public async Task UpdateAuthenticationSettings_Success_ReturnsOk()
    {
        // Arrange
        var settings = new AuthenticationSettings { AllowEmailLogin = true, AllowUsernameLogin = true, LoginMethod = LoginMethod.Both };
        _service.Setup(s => s.UpdateAuthenticationSettingsAsync(settings)).ReturnsAsync(true);
        var controller = new AuthenticationSettingsController(_service.Object, NullLogger<AuthenticationSettingsController>.Instance);

        // Act
        var result = await controller.UpdateAuthenticationSettings(settings);

        // Assert
        result.Result.ShouldBeOfType<OkObjectResult>();
    }
}
