using System;
using System.Reflection;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Common;
using PolyBucket.Api.Common.Http;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.Users.GeneratePasswordResetLink.Domain;
using PolyBucket.Api.Features.Users.GeneratePasswordResetLink.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Users.GeneratePasswordResetLink.Http;

public class GeneratePasswordResetLinkControllerTests
{
    private readonly Mock<IGeneratePasswordResetLinkService> _service = new();
    private readonly Guid _adminId = Guid.NewGuid();

    private GeneratePasswordResetLinkController CreateController(bool authenticated = true)
    {
        var identity = authenticated
            ? new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, _adminId.ToString()) }, "Test")
            : new ClaimsIdentity();
        return new GeneratePasswordResetLinkController(_service.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
            }
        };
    }

    [Fact(DisplayName = "When generating a link for an existing user, the controller returns the link and disables caching.")]
    public async Task Generate_ExistingUser_ShouldReturnOkWithNoStore()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expiresAt = DateTime.UtcNow.AddHours(24);
        _service.Setup(s => s.GenerateAsync(userId, _adminId, It.IsAny<ClientRequestInfo>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PasswordResetLinkResult("/reset-password?token=t", "https://x.example/reset-password?token=t", expiresAt));
        var controller = CreateController();

        // Act
        var result = await controller.GeneratePasswordResetLink(userId, CancellationToken.None);

        // Assert
        var response = result.ShouldBeOfType<OkObjectResult>().Value.ShouldBeOfType<GeneratePasswordResetLinkResponse>();
        response.Url.ShouldBe("https://x.example/reset-password?token=t");
        response.Path.ShouldBe("/reset-password?token=t");
        response.ExpiresAt.ShouldBe(expiresAt);
        controller.Response.Headers.CacheControl.ToString().ShouldBe("no-store");
    }

    [Fact(DisplayName = "When the user does not exist, the controller returns NotFound.")]
    public async Task Generate_UnknownUser_ShouldReturnNotFound()
    {
        // Arrange
        _service.Setup(s => s.GenerateAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<ClientRequestInfo>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException("User not found"));

        // Act
        var result = await CreateController().GeneratePasswordResetLink(Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.ShouldBeOfType<NotFoundObjectResult>();
    }

    [Fact(DisplayName = "When the caller has no user id claim, the controller returns Unauthorized.")]
    public async Task Generate_NoUserClaim_ShouldReturnUnauthorized()
    {
        // Arrange
        var controller = CreateController(authenticated: false);

        // Act
        var result = await controller.GeneratePasswordResetLink(Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.ShouldBeOfType<UnauthorizedObjectResult>();
    }

    [Fact(DisplayName = "The controller requires authentication and the manage users permission.")]
    public void Controller_ShouldRequireManageUsersPermission()
    {
        // Arrange
        var permissionsField = typeof(RequirePermissionAttribute).GetField("_permissions", BindingFlags.NonPublic | BindingFlags.Instance);

        // Act
        var authorize = typeof(GeneratePasswordResetLinkController).GetCustomAttribute<AuthorizeAttribute>();
        var permission = typeof(GeneratePasswordResetLinkController).GetCustomAttribute<RequirePermissionAttribute>();

        // Assert
        authorize.ShouldNotBeNull();
        ((string[])permissionsField!.GetValue(permission!)!).ShouldContain(PermissionConstants.ADMIN_MANAGE_USERS);
    }
}
