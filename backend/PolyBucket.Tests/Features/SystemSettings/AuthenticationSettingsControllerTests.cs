using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.SystemSettings.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.SystemSettings;

public class AuthenticationSettingsControllerTests
{
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
}
