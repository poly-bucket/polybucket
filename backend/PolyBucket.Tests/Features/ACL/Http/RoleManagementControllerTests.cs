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

public class RoleManagementControllerTests
{
    private readonly Mock<IRoleManagementService> _roleManagement = new();
    private readonly Mock<IPermissionService> _permissions = new();
    private readonly Guid _userId = Guid.NewGuid();

    private RoleManagementController CreateController(Guid? userId) =>
        new RoleManagementController(_roleManagement.Object, _permissions.Object).WithUser(userId);

    [Fact(DisplayName = "When a role exists, GetRole returns Ok with the role DTO.")]
    public async Task GetRole_Found_ReturnsOk()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var role = new Role { Id = roleId, Name = "Moderator", Description = "Mods", Priority = 50 };
        _roleManagement.Setup(s => s.GetRoleByIdAsync(roleId)).ReturnsAsync(role);
        _roleManagement.Setup(s => s.GetRolePermissionsAsync(roleId)).ReturnsAsync(new List<string> { "moderation.view" });
        _roleManagement.Setup(s => s.GetUserCountAsync(roleId)).ReturnsAsync(3);

        // Act
        var result = await CreateController(_userId).GetRole(roleId);

        // Assert
        var ok = result.Result.ShouldBeOfType<OkObjectResult>();
        var dto = ok.Value.ShouldBeOfType<RoleDto>();
        dto.Id.ShouldBe(roleId);
        dto.Name.ShouldBe("Moderator");
        dto.Permissions.ShouldContain("moderation.view");
        dto.UserCount.ShouldBe(3);
    }

    [Fact(DisplayName = "When a role does not exist, GetRole returns NotFound.")]
    public async Task GetRole_Missing_ReturnsNotFound()
    {
        // Arrange
        _roleManagement.Setup(s => s.GetRoleByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Role?)null);

        // Act
        var result = await CreateController(_userId).GetRole(Guid.NewGuid());

        // Assert
        result.Result.ShouldBeOfType<NotFoundObjectResult>();
    }

    [Fact(DisplayName = "When creating a role without a user id, CreateRole returns Unauthorized.")]
    public async Task CreateRole_NoUser_ReturnsUnauthorized()
    {
        // Arrange
        var request = new CreateRoleRequest { Name = "Custom", Description = "Desc", Priority = 10 };

        // Act
        var result = await CreateController(null).CreateRole(request);

        // Assert
        result.Result.ShouldBeOfType<UnauthorizedResult>();
        _roleManagement.Verify(s => s.CreateRoleAsync(It.IsAny<Role>(), It.IsAny<List<string>>()), Times.Never);
    }

    [Fact(DisplayName = "When role creation fails validation, CreateRole returns BadRequest.")]
    public async Task CreateRole_InvalidOperation_ReturnsBadRequest()
    {
        // Arrange
        var request = new CreateRoleRequest { Name = "Dup", Description = "Desc", Priority = 10 };
        _roleManagement.Setup(s => s.CreateRoleAsync(It.IsAny<Role>(), It.IsAny<List<string>>()))
            .ThrowsAsync(new InvalidOperationException("Role name already exists"));

        // Act
        var result = await CreateController(_userId).CreateRole(request);

        // Assert
        result.Result.ShouldBeOfType<BadRequestObjectResult>();
    }

    [Fact(DisplayName = "When the caller cannot manage a role, UpdateRole returns Forbid.")]
    public async Task UpdateRole_CannotManage_ReturnsForbid()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        _roleManagement.Setup(s => s.CanManageRoleAsync(_userId, roleId)).ReturnsAsync(false);

        // Act
        var result = await CreateController(_userId).UpdateRole(roleId, new UpdateRoleRequest { Name = "X" });

        // Assert
        result.Result.ShouldBeOfType<ForbidResult>();
    }

    [Fact(DisplayName = "When permissions are listed, GetAllPermissions returns categorized DTOs.")]
    public async Task GetAllPermissions_ReturnsOk()
    {
        // Arrange
        _permissions.Setup(s => s.GetAllPermissionsAsync()).ReturnsAsync(new List<Permission>
        {
            new() { Name = "admin.manage_users" }
        });

        // Act
        var result = await CreateController(_userId).GetAllPermissions();

        // Assert
        var ok = result.Result.ShouldBeOfType<OkObjectResult>();
        var list = ok.Value.ShouldBeOfType<List<PermissionDto>>();
        list.ShouldNotBeEmpty();
        list[0].Name.ShouldBe("admin.manage_users");
    }
}
