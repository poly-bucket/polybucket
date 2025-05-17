using System;
using System.Linq;
using System.Threading.Tasks;
using Core.Models.Roles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Core.Interfaces;
using Core.Models.Roles;
using Infrastructure.Data;
using Infrastructure.Services;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests
{
    // Simple test context for Role entities only
    public class RoleTestContext : DbContext
    {
        public DbSet<Role> Roles { get; set; }

        public RoleTestContext(DbContextOptions<RoleTestContext> options)
            : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.CoreEventId.NavigationBaseIncludeIgnored));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Role>()
                .Property(r => r.Name)
                .IsRequired();
                
            modelBuilder.Entity<Role>()
                .Property(r => r.Description)
                .IsRequired(false);
                
            // Configure soft delete query filter
            modelBuilder.Entity<Role>().HasQueryFilter(r => r.DeletedAt == null);
        }
    }

    public class RoleTests
    {
        [Fact]
        public void Role_ShouldHaveCorrectProperties()
        {
            // Arrange & Act
            var role = new Role
            {
                Name = "Admin",
                Description = "Administrator role",
                IsSystemRole = true
            };

            // Assert
            role.Id.ShouldNotBe(Guid.Empty);
            role.Name.ShouldBe("Admin");
            role.Description.ShouldBe("Administrator role");
            role.IsSystemRole.ShouldBeTrue();
            role.CreatedAt.ShouldNotBe(default);
            role.UpdatedAt.ShouldNotBe(default);
            role.DeletedAt.ShouldBeNull();
            role.IsDeleted.ShouldBeFalse();
        }

        [Fact]
        public void Role_IsDeleted_ShouldBeTrueWhenDeletedAtIsSet()
        {
            // Arrange
            var role = new Role
            {
                Name = "TestRole",
                Description = "Test role"
            };

            // Act
            role.DeletedAt = DateTime.UtcNow;

            // Assert
            role.IsDeleted.ShouldBeTrue();
        }

        [Fact]
        public async Task RoleService_GetAllRolesAsync_ShouldReturnAllRoles()
        {
            // Arrange
            var mockRepository = new Mock<IRoleRepository>();
            var mockLogger = new Mock<ILogger<RoleService>>();

            var roles = new[]
            {
                new Role { Name = "Admin", Description = "Administrator role", IsSystemRole = true },
                new Role { Name = "User", Description = "Regular user role", IsSystemRole = true },
                new Role { Name = "Custom", Description = "Custom role", IsSystemRole = false }
            };

            mockRepository.Setup(r => r.GetAllAsync(default))
                .ReturnsAsync(roles);

            var service = new RoleService(mockRepository.Object, mockLogger.Object);

            // Act
            var result = await service.GetAllRolesAsync();

            // Assert
            result.ShouldNotBeNull();
            result.Count().ShouldBe(3);
        }

        [Fact]
        public async Task SoftDeleteQueryFilter_ShouldExcludeDeletedRoles()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<RoleTestContext>()
                .UseInMemoryDatabase(databaseName: "SoftDeleteQueryFilter_Test")
                .Options;

            var roleId = Guid.NewGuid();
            var role = new Role
            {
                Id = roleId,
                Name = "RoleToDelete",
                Description = "Role to be deleted"
            };

            // Add test data
            using (var context = new RoleTestContext(options))
            {
                context.Roles.Add(role);
                await context.SaveChangesAsync();
            }

            // First verify the role exists
            using (var context = new RoleTestContext(options))
            {
                var roles = await context.Roles.ToListAsync();
                roles.Count.ShouldBe(1);
                roles.Any(r => r.Id == roleId).ShouldBeTrue();
            }

            // Now soft delete the role
            using (var context = new RoleTestContext(options))
            {
                var roleToDelete = await context.Roles.FindAsync(roleId);
                roleToDelete.ShouldNotBeNull();
                roleToDelete.DeletedAt = DateTime.UtcNow;
                await context.SaveChangesAsync();
            }

            // Verify the role is excluded by the query filter
            using (var context = new RoleTestContext(options))
            {
                var roles = await context.Roles.ToListAsync();
                roles.Count.ShouldBe(0);
                
                // But we can still find it by ignoring the query filter
                var deletedRoles = await context.Roles.IgnoreQueryFilters().ToListAsync();
                deletedRoles.Count.ShouldBe(1);
                deletedRoles.First().Id.ShouldBe(roleId);
                deletedRoles.First().DeletedAt.ShouldNotBeNull();
            }
        }
    }
}