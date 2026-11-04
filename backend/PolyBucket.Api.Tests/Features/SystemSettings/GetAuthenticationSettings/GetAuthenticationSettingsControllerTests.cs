using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.SystemSettings.Domain;
using PolyBucket.Api.Features.SystemSettings.GetAuthenticationSettings.Domain;
using PolyBucket.Api.Features.SystemSettings.GetAuthenticationSettings.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.SystemSettings.GetAuthenticationSettings;

public class GetAuthenticationSettingsControllerTests
{
    [Fact(DisplayName = "The get authentication settings controller requires the system settings permission.")]
    public void Controller_ShouldRequireSystemSettingsPermission()
    {
        // Arrange
        var permissionsField = typeof(RequirePermissionAttribute).GetField("_permissions", BindingFlags.NonPublic | BindingFlags.Instance);

        // Act
        var authorizeAttr = typeof(GetAuthenticationSettingsController).GetCustomAttribute<AuthorizeAttribute>();
        var permissionAttr = typeof(GetAuthenticationSettingsController).GetCustomAttribute<RequirePermissionAttribute>();

        // Assert
        authorizeAttr.ShouldNotBeNull();
        permissionAttr.ShouldNotBeNull();
        var permissions = (string[])permissionsField!.GetValue(permissionAttr!)!;
        permissions.ShouldContain(PermissionConstants.ADMIN_SYSTEM_SETTINGS);
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
