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
using PolyBucket.Api.Features.Users.MarkEmailVerified.Domain;
using PolyBucket.Api.Features.Users.MarkEmailVerified.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Users.MarkEmailVerified.Http;

public class MarkEmailVerifiedControllerTests
{
    private readonly Mock<IMarkEmailVerifiedService> _service = new();
    private readonly Guid _adminId = Guid.NewGuid();

    private MarkEmailVerifiedController CreateController(bool authenticated = true)
    {
        var identity = authenticated
            ? new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, _adminId.ToString()) }, "Test")
            : new ClaimsIdentity();
        return new MarkEmailVerifiedController(_service.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
            }
        };
    }

    [Fact(DisplayName = "When marking an existing user verified, the controller returns Ok with the verification time.")]
    public async Task MarkEmailVerified_ExistingUser_ShouldReturnOk()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var verifiedAt = DateTime.UtcNow;
        _service.Setup(s => s.MarkEmailVerifiedAsync(userId, _adminId, It.IsAny<ClientRequestInfo>(), It.IsAny<CancellationToken>())).ReturnsAsync(verifiedAt);

        // Act
        var result = await CreateController().MarkEmailVerified(userId, CancellationToken.None);

        // Assert
        var ok = result.ShouldBeOfType<OkObjectResult>();
        ok.Value.ShouldBeOfType<MarkEmailVerifiedResponse>().EmailVerifiedAt.ShouldBe(verifiedAt);
    }

    [Fact(DisplayName = "When the user does not exist, the controller returns NotFound.")]
    public async Task MarkEmailVerified_UnknownUser_ShouldReturnNotFound()
    {
        // Arrange
        _service.Setup(s => s.MarkEmailVerifiedAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<ClientRequestInfo>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException("User not found"));

        // Act
        var result = await CreateController().MarkEmailVerified(Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.ShouldBeOfType<NotFoundObjectResult>();
    }

    [Fact(DisplayName = "When the caller has no user id claim, the controller returns Unauthorized.")]
    public async Task MarkEmailVerified_NoUserClaim_ShouldReturnUnauthorized()
    {
        // Arrange
        var controller = CreateController(authenticated: false);

        // Act
        var result = await controller.MarkEmailVerified(Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.ShouldBeOfType<UnauthorizedObjectResult>();
        _service.VerifyNoOtherCalls();
    }

    [Fact(DisplayName = "The controller requires authentication and the manage users permission.")]
    public void Controller_ShouldRequireManageUsersPermission()
    {
        // Arrange
        var permissionsField = typeof(RequirePermissionAttribute).GetField("_permissions", BindingFlags.NonPublic | BindingFlags.Instance);

        // Act
        var authorize = typeof(MarkEmailVerifiedController).GetCustomAttribute<AuthorizeAttribute>();
        var permission = typeof(MarkEmailVerifiedController).GetCustomAttribute<RequirePermissionAttribute>();

        // Assert
        authorize.ShouldNotBeNull();
        ((string[])permissionsField!.GetValue(permission!)!).ShouldContain(PermissionConstants.ADMIN_MANAGE_USERS);
    }
}
