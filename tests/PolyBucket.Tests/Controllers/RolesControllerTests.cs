using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using PolyBucket.Api.Controllers;
using PolyBucket.Core.Interfaces;
using PolyBucket.Core.Models.Roles;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace PolyBucket.Tests.Controllers
{
    public class RolesControllerTests
    {
        private readonly Mock<IRoleService> _mockRoleService;
        private readonly Mock<ILogger<RolesController>> _mockLogger;
        private readonly RolesController _controller;

        public RolesControllerTests()
        {
            _mockRoleService = new Mock<IRoleService>();
            _mockLogger = new Mock<ILogger<RolesController>>();
            _controller = new RolesController(_mockRoleService.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetRoles_ShouldReturnOkResult_WithRoles()
        {
            // Arrange
            var roles = new List<RoleDto>
            {
                new RoleDto 
                { 
                    Id = Guid.NewGuid(), 
                    Name = "Admin", 
                    Description = "Administrator Role", 
                    IsSystemRole = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new RoleDto 
                { 
                    Id = Guid.NewGuid(), 
                    Name = "User", 
                    Description = "Regular User Role", 
                    IsSystemRole = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };

            _mockRoleService.Setup(service => service.GetAllRolesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(roles);

            // Act
            var result = await _controller.GetRoles(CancellationToken.None);

            // Assert
            var okResult = result.Result.ShouldBeOfType<OkObjectResult>();
            var returnedRoles = okResult.Value.ShouldBeOfType<List<RoleDto>>();
            returnedRoles.Count.ShouldBe(2);
            returnedRoles.Any(r => r.Name == "Admin").ShouldBeTrue();
            returnedRoles.Any(r => r.Name == "User").ShouldBeTrue();
        }

        [Fact]
        public async Task GetRole_ShouldReturnOkResult_WhenRoleExists()
        {
            // Arrange
            var roleId = Guid.NewGuid();
            var role = new RoleDto
            {
                Id = roleId,
                Name = "Admin",
                Description = "Administrator Role",
                IsSystemRole = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _mockRoleService.Setup(service => service.GetRoleByIdAsync(roleId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(role);

            // Act
            var result = await _controller.GetRole(roleId, CancellationToken.None);

            // Assert
            var okResult = result.Result.ShouldBeOfType<OkObjectResult>();
            var returnedRole = okResult.Value.ShouldBeOfType<RoleDto>();
            returnedRole.Id.ShouldBe(roleId);
            returnedRole.Name.ShouldBe("Admin");
        }

        [Fact]
        public async Task GetRole_ShouldReturnNotFound_WhenRoleDoesNotExist()
        {
            // Arrange
            var roleId = Guid.NewGuid();

            _mockRoleService.Setup(service => service.GetRoleByIdAsync(roleId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((RoleDto)null);

            // Act
            var result = await _controller.GetRole(roleId, CancellationToken.None);

            // Assert
            result.Result.ShouldBeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task CreateRole_ShouldReturnCreatedAtAction_WhenRoleIsCreated()
        {
            // Arrange
            var request = new CreateRoleRequest
            {
                Name = "NewRole",
                Description = "New Role Description"
            };

            var createdRole = new RoleDto
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                IsSystemRole = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _mockRoleService.Setup(service => service.CreateRoleAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(createdRole);

            // Act
            var result = await _controller.CreateRole(request, CancellationToken.None);

            // Assert
            var createdAtActionResult = result.Result.ShouldBeOfType<CreatedAtActionResult>();
            createdAtActionResult.ActionName.ShouldBe(nameof(RolesController.GetRole));
            createdAtActionResult.RouteValues["id"].ShouldBe(createdRole.Id);
            var returnedRole = createdAtActionResult.Value.ShouldBeOfType<RoleDto>();
            returnedRole.Id.ShouldBe(createdRole.Id);
            returnedRole.Name.ShouldBe(request.Name);
            returnedRole.Description.ShouldBe(request.Description);
        }

        [Fact]
        public async Task CreateRole_ShouldReturnBadRequest_WhenRoleWithSameNameExists()
        {
            // Arrange
            var request = new CreateRoleRequest
            {
                Name = "ExistingRole",
                Description = "Existing Role Description"
            };

            _mockRoleService.Setup(service => service.CreateRoleAsync(request, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException($"Role with name '{request.Name}' already exists"));

            // Act
            var result = await _controller.CreateRole(request, CancellationToken.None);

            // Assert
            var badRequestResult = result.Result.ShouldBeOfType<BadRequestObjectResult>();
            badRequestResult.Value.ShouldNotBeNull();
            // Get the anonymous type's properties dynamically
            var errorProperty = badRequestResult.Value.GetType().GetProperty("error");
            errorProperty.ShouldNotBeNull();
            errorProperty.GetValue(badRequestResult.Value).ShouldBe($"Role with name '{request.Name}' already exists");
        }

        [Fact]
        public async Task UpdateRole_ShouldReturnOkResult_WhenRoleIsUpdated()
        {
            // Arrange
            var roleId = Guid.NewGuid();
            var request = new UpdateRoleRequest
            {
                Name = "UpdatedRole",
                Description = "Updated Description"
            };

            var updatedRole = new RoleDto
            {
                Id = roleId,
                Name = request.Name,
                Description = request.Description,
                IsSystemRole = false,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow
            };

            _mockRoleService.Setup(service => service.UpdateRoleAsync(roleId, request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(updatedRole);

            // Act
            var result = await _controller.UpdateRole(roleId, request, CancellationToken.None);

            // Assert
            var okResult = result.Result.ShouldBeOfType<OkObjectResult>();
            var returnedRole = okResult.Value.ShouldBeOfType<RoleDto>();
            returnedRole.Id.ShouldBe(roleId);
            returnedRole.Name.ShouldBe(request.Name);
            returnedRole.Description.ShouldBe(request.Description);
        }

        [Fact]
        public async Task UpdateRole_ShouldReturnNotFound_WhenRoleDoesNotExist()
        {
            // Arrange
            var roleId = Guid.NewGuid();
            var request = new UpdateRoleRequest
            {
                Name = "UpdatedRole",
                Description = "Updated Description"
            };

            _mockRoleService.Setup(service => service.UpdateRoleAsync(roleId, request, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new KeyNotFoundException($"Role with ID {roleId} not found"));

            // Act
            var result = await _controller.UpdateRole(roleId, request, CancellationToken.None);

            // Assert
            result.Result.ShouldBeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task UpdateRole_ShouldReturnBadRequest_WhenRoleIsSystemRole()
        {
            // Arrange
            var roleId = Guid.NewGuid();
            var request = new UpdateRoleRequest
            {
                Name = "UpdatedRole",
                Description = "Updated Description"
            };

            _mockRoleService.Setup(service => service.UpdateRoleAsync(roleId, request, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("System roles cannot be modified"));

            // Act
            var result = await _controller.UpdateRole(roleId, request, CancellationToken.None);

            // Assert
            var badRequestResult = result.Result.ShouldBeOfType<BadRequestObjectResult>();
            badRequestResult.Value.ShouldNotBeNull();
            // Get the anonymous type's properties dynamically
            var errorProperty = badRequestResult.Value.GetType().GetProperty("error");
            errorProperty.ShouldNotBeNull();
            errorProperty.GetValue(badRequestResult.Value).ShouldBe("System roles cannot be modified");
        }

        [Fact]
        public async Task DeleteRole_ShouldReturnNoContent_WhenRoleIsDeleted()
        {
            // Arrange
            var roleId = Guid.NewGuid();

            _mockRoleService.Setup(service => service.DeleteRoleAsync(roleId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteRole(roleId, CancellationToken.None);

            // Assert
            result.ShouldBeOfType<NoContentResult>();
        }

        [Fact]
        public async Task DeleteRole_ShouldReturnNotFound_WhenRoleDoesNotExist()
        {
            // Arrange
            var roleId = Guid.NewGuid();

            _mockRoleService.Setup(service => service.DeleteRoleAsync(roleId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.DeleteRole(roleId, CancellationToken.None);

            // Assert
            result.ShouldBeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task DeleteRole_ShouldReturnBadRequest_WhenRoleIsSystemRole()
        {
            // Arrange
            var roleId = Guid.NewGuid();

            _mockRoleService.Setup(service => service.DeleteRoleAsync(roleId, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("System roles cannot be deleted"));

            // Act
            var result = await _controller.DeleteRole(roleId, CancellationToken.None);

            // Assert
            var badRequestResult = result.ShouldBeOfType<BadRequestObjectResult>();
            badRequestResult.Value.ShouldNotBeNull();
            // Get the anonymous type's properties dynamically
            var errorProperty = badRequestResult.Value.GetType().GetProperty("error");
            errorProperty.ShouldNotBeNull();
            errorProperty.GetValue(badRequestResult.Value).ShouldBe("System roles cannot be deleted");
        }
    }
} 