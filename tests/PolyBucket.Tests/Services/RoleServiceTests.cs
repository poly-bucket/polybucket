using Microsoft.Extensions.Logging;
using Moq;
using PolyBucket.Core.Entities;
using PolyBucket.Core.Interfaces;
using PolyBucket.Core.Models.Roles;
using PolyBucket.Infrastructure.Services;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace PolyBucket.Tests.Services
{
    public class RoleServiceTests
    {
        private readonly Mock<IRoleRepository> _mockRoleRepository;
        private readonly Mock<ILogger<RoleService>> _mockLogger;
        private readonly RoleService _roleService;

        public RoleServiceTests()
        {
            _mockRoleRepository = new Mock<IRoleRepository>();
            _mockLogger = new Mock<ILogger<RoleService>>();
            _roleService = new RoleService(_mockRoleRepository.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetAllRolesAsync_ShouldReturnMappedRoleDtos()
        {
            // Arrange
            var roles = new List<Role>
            {
                new Role 
                { 
                    Id = Guid.NewGuid(), 
                    Name = "Admin", 
                    Description = "Administrator Role", 
                    IsSystemRole = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Role 
                { 
                    Id = Guid.NewGuid(), 
                    Name = "User", 
                    Description = "Regular User Role", 
                    IsSystemRole = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };

            _mockRoleRepository.Setup(repo => repo.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(roles);

            // Act
            var result = await _roleService.GetAllRolesAsync();

            // Assert
            result.ShouldNotBeNull();
            result.Count().ShouldBe(2);
            
            // Verify the DTOs are correctly mapped
            var adminRole = result.First(r => r.Name == "Admin");
            adminRole.IsSystemRole.ShouldBeTrue();
            adminRole.Name.ShouldBe("Admin");
            adminRole.Description.ShouldBe("Administrator Role");
            
            var userRole = result.First(r => r.Name == "User");
            userRole.IsSystemRole.ShouldBeFalse();
            userRole.Name.ShouldBe("User");
            userRole.Description.ShouldBe("Regular User Role");
        }

        [Fact]
        public async Task GetRoleByIdAsync_ShouldReturnMappedRoleDto_WhenRoleExists()
        {
            // Arrange
            var roleId = Guid.NewGuid();
            var role = new Role
            {
                Id = roleId,
                Name = "Admin",
                Description = "Administrator Role",
                IsSystemRole = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _mockRoleRepository.Setup(repo => repo.GetByIdAsync(roleId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(role);

            // Act
            var result = await _roleService.GetRoleByIdAsync(roleId);

            // Assert
            result.ShouldNotBeNull();
            result.Id.ShouldBe(roleId);
            result.Name.ShouldBe("Admin");
            result.IsSystemRole.ShouldBeTrue();
        }

        [Fact]
        public async Task GetRoleByIdAsync_ShouldReturnNull_WhenRoleDoesNotExist()
        {
            // Arrange
            var roleId = Guid.NewGuid();

            _mockRoleRepository.Setup(repo => repo.GetByIdAsync(roleId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Role)null);

            // Act
            var result = await _roleService.GetRoleByIdAsync(roleId);

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public async Task CreateRoleAsync_ShouldCreateAndReturnRole_WhenNameIsUnique()
        {
            // Arrange
            var request = new CreateRoleRequest
            {
                Name = "NewRole",
                Description = "New Role Description"
            };

            _mockRoleRepository.Setup(repo => repo.ExistsByNameAsync(request.Name, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _mockRoleRepository.Setup(repo => repo.CreateAsync(It.IsAny<Role>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Role role, CancellationToken token) => role);

            // Act
            var result = await _roleService.CreateRoleAsync(request);

            // Assert
            result.ShouldNotBeNull();
            result.Name.ShouldBe("NewRole");
            result.Description.ShouldBe("New Role Description");
            result.IsSystemRole.ShouldBeFalse();

            // Verify repository was called with correct data
            _mockRoleRepository.Verify(repo => repo.ExistsByNameAsync(request.Name, It.IsAny<CancellationToken>()), Times.Once);
            _mockRoleRepository.Verify(repo => repo.CreateAsync(
                It.Is<Role>(r => 
                    r.Name == request.Name && 
                    r.Description == request.Description && 
                    r.IsSystemRole == false), 
                It.IsAny<CancellationToken>()), 
                Times.Once);
        }

        [Fact]
        public async Task CreateRoleAsync_ShouldThrowException_WhenNameAlreadyExists()
        {
            // Arrange
            var request = new CreateRoleRequest
            {
                Name = "ExistingRole",
                Description = "Existing Role Description"
            };

            _mockRoleRepository.Setup(repo => repo.ExistsByNameAsync(request.Name, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act & Assert
            await Should.ThrowAsync<InvalidOperationException>(async () =>
            {
                await _roleService.CreateRoleAsync(request);
            });

            // Verify repository was called with correct data
            _mockRoleRepository.Verify(repo => repo.ExistsByNameAsync(request.Name, It.IsAny<CancellationToken>()), Times.Once);
            _mockRoleRepository.Verify(repo => repo.CreateAsync(It.IsAny<Role>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task UpdateRoleAsync_ShouldUpdateAndReturnRole_WhenRoleExistsAndIsNotSystemRole()
        {
            // Arrange
            var roleId = Guid.NewGuid();
            var existingRole = new Role
            {
                Id = roleId,
                Name = "OriginalRole",
                Description = "Original Description",
                IsSystemRole = false,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            };

            var request = new UpdateRoleRequest
            {
                Name = "UpdatedRole",
                Description = "Updated Description"
            };

            _mockRoleRepository.Setup(repo => repo.GetByIdAsync(roleId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingRole);

            _mockRoleRepository.Setup(repo => repo.ExistsByNameAsync(request.Name, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _mockRoleRepository.Setup(repo => repo.UpdateAsync(It.IsAny<Role>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Role role, CancellationToken token) => role);

            // Act
            var result = await _roleService.UpdateRoleAsync(roleId, request);

            // Assert
            result.ShouldNotBeNull();
            result.Id.ShouldBe(roleId);
            result.Name.ShouldBe("UpdatedRole");
            result.Description.ShouldBe("Updated Description");

            // Verify repository was called with correct data
            _mockRoleRepository.Verify(repo => repo.GetByIdAsync(roleId, It.IsAny<CancellationToken>()), Times.Once);
            _mockRoleRepository.Verify(repo => repo.ExistsByNameAsync(request.Name, It.IsAny<CancellationToken>()), Times.Once);
            _mockRoleRepository.Verify(repo => repo.UpdateAsync(
                It.Is<Role>(r => 
                    r.Id == roleId && 
                    r.Name == request.Name && 
                    r.Description == request.Description), 
                It.IsAny<CancellationToken>()), 
                Times.Once);
        }

        [Fact]
        public async Task UpdateRoleAsync_ShouldThrowException_WhenRoleIsSystemRole()
        {
            // Arrange
            var roleId = Guid.NewGuid();
            var systemRole = new Role
            {
                Id = roleId,
                Name = "Administrator",
                Description = "System Administrator Role",
                IsSystemRole = true
            };

            var request = new UpdateRoleRequest
            {
                Name = "UpdatedRole",
                Description = "Updated Description"
            };

            _mockRoleRepository.Setup(repo => repo.GetByIdAsync(roleId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(systemRole);

            // Act & Assert
            await Should.ThrowAsync<InvalidOperationException>(async () =>
            {
                await _roleService.UpdateRoleAsync(roleId, request);
            });

            // Verify repository was called correctly
            _mockRoleRepository.Verify(repo => repo.GetByIdAsync(roleId, It.IsAny<CancellationToken>()), Times.Once);
            _mockRoleRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Role>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task UpdateRoleAsync_ShouldThrowException_WhenRoleDoesNotExist()
        {
            // Arrange
            var roleId = Guid.NewGuid();
            var request = new UpdateRoleRequest
            {
                Name = "UpdatedRole",
                Description = "Updated Description"
            };

            _mockRoleRepository.Setup(repo => repo.GetByIdAsync(roleId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Role)null);

            // Act & Assert
            await Should.ThrowAsync<KeyNotFoundException>(async () =>
            {
                await _roleService.UpdateRoleAsync(roleId, request);
            });

            // Verify repository was called correctly
            _mockRoleRepository.Verify(repo => repo.GetByIdAsync(roleId, It.IsAny<CancellationToken>()), Times.Once);
            _mockRoleRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Role>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task DeleteRoleAsync_ShouldDeleteAndReturnTrue_WhenRoleExistsAndIsNotSystemRole()
        {
            // Arrange
            var roleId = Guid.NewGuid();
            var role = new Role
            {
                Id = roleId,
                Name = "RoleToDelete",
                Description = "Role to be deleted",
                IsSystemRole = false
            };

            _mockRoleRepository.Setup(repo => repo.GetByIdAsync(roleId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(role);

            _mockRoleRepository.Setup(repo => repo.SoftDeleteAsync(roleId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _roleService.DeleteRoleAsync(roleId);

            // Assert
            result.ShouldBeTrue();

            // Verify repository was called correctly
            _mockRoleRepository.Verify(repo => repo.GetByIdAsync(roleId, It.IsAny<CancellationToken>()), Times.Once);
            _mockRoleRepository.Verify(repo => repo.SoftDeleteAsync(roleId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteRoleAsync_ShouldThrowException_WhenRoleIsSystemRole()
        {
            // Arrange
            var roleId = Guid.NewGuid();
            var systemRole = new Role
            {
                Id = roleId,
                Name = "Administrator",
                Description = "System Administrator Role",
                IsSystemRole = true
            };

            _mockRoleRepository.Setup(repo => repo.GetByIdAsync(roleId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(systemRole);

            // Act & Assert
            await Should.ThrowAsync<InvalidOperationException>(async () =>
            {
                await _roleService.DeleteRoleAsync(roleId);
            });

            // Verify repository was called correctly
            _mockRoleRepository.Verify(repo => repo.GetByIdAsync(roleId, It.IsAny<CancellationToken>()), Times.Once);
            _mockRoleRepository.Verify(repo => repo.SoftDeleteAsync(roleId, It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task DeleteRoleAsync_ShouldReturnFalse_WhenRoleDoesNotExist()
        {
            // Arrange
            var roleId = Guid.NewGuid();

            _mockRoleRepository.Setup(repo => repo.GetByIdAsync(roleId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Role)null);

            // Act
            var result = await _roleService.DeleteRoleAsync(roleId);

            // Assert
            result.ShouldBeFalse();

            // Verify repository was called correctly
            _mockRoleRepository.Verify(repo => repo.GetByIdAsync(roleId, It.IsAny<CancellationToken>()), Times.Once);
            _mockRoleRepository.Verify(repo => repo.SoftDeleteAsync(roleId, It.IsAny<CancellationToken>()), Times.Never);
        }
    }
} 