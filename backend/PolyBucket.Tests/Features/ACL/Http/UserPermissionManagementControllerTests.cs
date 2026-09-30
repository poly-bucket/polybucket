using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.ACL.Http;
using PolyBucket.Api.Features.ACL.Services;
using PolyBucket.Tests.Testing;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.ACL.Http;

public class UserPermissionManagementControllerTests
{
    private readonly Mock<IPermissionService> _permissions = new();
    private readonly Guid _userId = Guid.NewGuid();

    private UserPermissionManagementController CreateController(Guid? userId) =>
        new UserPermissionManagementController(_permissions.Object).WithUser(userId);

    [Fact(DisplayName = "When the current user has a role, GetCurrentUserPermissions returns Ok.")]
    public async Task GetCurrentUserPermissions_ValidUser_ReturnsOk()
    {
        // Arrange
        var role = new Role { Id = Guid.NewGuid(), Name = "User", Priority = 100 };
        _permissions.Setup(s => s.GetUserRoleAsync(_userId)).ReturnsAsync(role);
        _permissions.Setup(s => s.GetUserPermissionsAsync(_userId)).ReturnsAsync(new List<string> { "model.view" });
        _permissions.Setup(s => s.GetUserPermissionOverridesAsync(_userId)).ReturnsAsync(new List<UserPermission>());

        // Act
        var result = await CreateController(_userId).GetCurrentUserPermissions();

        // Assert
        var ok = result.Result.ShouldBeOfType<OkObjectResult>();
        var dto = ok.Value.ShouldBeOfType<UserPermissionsDto>();
        dto.UserId.ShouldBe(_userId);
        dto.Role.Name.ShouldBe("User");
        dto.EffectivePermissions.ShouldContain("model.view");
    }

    [Fact(DisplayName = "When the current user has no user id claim, GetCurrentUserPermissions returns Unauthorized.")]
    public async Task GetCurrentUserPermissions_NoClaim_ReturnsUnauthorized()
    {
        // Act
        var result = await CreateController(null).GetCurrentUserPermissions();

        // Assert
        result.Result.ShouldBeOfType<UnauthorizedResult>();
        _permissions.VerifyNoOtherCalls();
    }

    [Fact(DisplayName = "When the target user is unknown, GetUserPermissions returns NotFound.")]
    public async Task GetUserPermissions_UnknownUser_ReturnsNotFound()
    {
        // Arrange
        _permissions.Setup(s => s.GetUserRoleAsync(It.IsAny<Guid>())).ReturnsAsync((Role?)null);

        // Act
        var result = await CreateController(_userId).GetUserPermissions(Guid.NewGuid());

        // Assert
        result.Result.ShouldBeOfType<NotFoundObjectResult>();
    }

    [Fact(DisplayName = "When the assigner cannot manage the role, AssignUserRole returns Forbid.")]
    public async Task AssignUserRole_CannotManageRole_ReturnsForbid()
    {
        // Arrange
        var targetUserId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        _permissions.Setup(s => s.CanManageRoleAsync(_userId, roleId)).ReturnsAsync(false);

        // Act
        var result = await CreateController(_userId).AssignUserRole(targetUserId, new AssignRoleRequest { RoleId = roleId });

        // Assert
        result.ShouldBeOfType<ForbidResult>();
    }

    [Fact(DisplayName = "When granting admin permissions as a non-admin, GrantUserPermission returns Forbid.")]
    public async Task GrantUserPermission_NonAdminGrantingAdmin_ReturnsForbid()
    {
        // Arrange
        _permissions.Setup(s => s.IsAdminAsync(_userId)).ReturnsAsync(false);

        // Act
        var result = await CreateController(_userId).GrantUserPermission(
            Guid.NewGuid(),
            new GrantPermissionRequest { Permission = "admin.manage_users" });

        // Assert
        result.ShouldBeOfType<ForbidResult>();
    }

    [Fact(DisplayName = "When moderation capability is checked, CanUserModerateUser returns the service result.")]
    public async Task CanUserModerateUser_ReturnsDto()
    {
        // Arrange
        var moderatorId = Guid.NewGuid();
        var targetId = Guid.NewGuid();
        _permissions.Setup(s => s.CanUserModerateUserAsync(moderatorId, targetId)).ReturnsAsync(true);
        _permissions.Setup(s => s.GetUserRoleAsync(moderatorId)).ReturnsAsync(new Role { Name = "Admin" });
        _permissions.Setup(s => s.GetUserRoleAsync(targetId)).ReturnsAsync(new Role { Name = "User" });

        // Act
        var result = await CreateController(_userId).CanUserModerateUser(moderatorId, targetId);

        // Assert
        var ok = result.Result.ShouldBeOfType<OkObjectResult>();
        ok.Value.ShouldBeOfType<ModerationCheckDto>().CanModerate.ShouldBeTrue();
    }
}
