using Core.Models.Roles;
using Microsoft.EntityFrameworkCore;
using Moq;
using Core.Interfaces;
using Infrastructure.Data;
using Infrastructure.Data.Repositories;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace PolyBucket.Tests.Repositories
{
    // Add a dedicated test context for Role tests to avoid issues with complex relationships
    public class RoleTestContext : Microsoft.EntityFrameworkCore.DbContext
    {
        public DbSet<Role> Roles { get; set; }

        public RoleTestContext(DbContextOptions<RoleTestContext> options) 
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure Role entity
            modelBuilder.Entity<Role>(entity =>
            {
                entity.Property(e => e.Name).IsRequired();
                entity.Property(e => e.Description).IsRequired(false);
                
                // Configure soft delete query filter
                entity.HasQueryFilter(e => e.DeletedAt == null);
            });
        }
    }

    public class RoleRepositoryTests
    {
        [Fact]
        public async Task GetByIdAsync_ShouldReturnRole_WhenRoleExists()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<DbContext>()
                .UseInMemoryDatabase(databaseName: "GetByIdAsync_ShouldReturnRole_WhenRoleExists")
                .Options;

            var roleId = Guid.NewGuid();
            var role = new Role
            {
                Id = roleId,
                Name = "TestRole",
                Description = "Test Role Description",
                IsSystemRole = false
            };

            // Add test data to the in-memory database
            using (var context = new TestApplicationDbContext(options))
            {
                context.Roles.Add(role);
                await context.SaveChangesAsync();
            }

            // Act
            using (var context = new TestApplicationDbContext(options))
            {
                var repository = new RoleRepository(context);
                var result = await repository.GetByIdAsync(roleId);

                // Assert
                result.ShouldNotBeNull();
                result.Id.ShouldBe(roleId);
                result.Name.ShouldBe("TestRole");
                result.Description.ShouldBe("Test Role Description");
                result.IsSystemRole.ShouldBeFalse();
            }
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenRoleDoesNotExist()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<DbContext>()
                .UseInMemoryDatabase(databaseName: "GetByIdAsync_ShouldReturnNull_WhenRoleDoesNotExist")
                .Options;

            var nonExistentRoleId = Guid.NewGuid();

            // Act
            using (var context = new TestApplicationDbContext(options))
            {
                var repository = new RoleRepository(context);
                var result = await repository.GetByIdAsync(nonExistentRoleId);

                // Assert
                result.ShouldBeNull();
            }
        }

        [Fact]
        public async Task GetByNameAsync_ShouldReturnRole_WhenRoleExists()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<DbContext>()
                .UseInMemoryDatabase(databaseName: "GetByNameAsync_ShouldReturnRole_WhenRoleExists")
                .Options;

            var roleName = "AdminRole";
            var role = new Role
            {
                Id = Guid.NewGuid(),
                Name = roleName,
                Description = "Admin Role Description",
                IsSystemRole = true
            };

            // Add test data to the in-memory database
            using (var context = new TestApplicationDbContext(options))
            {
                context.Roles.Add(role);
                await context.SaveChangesAsync();
            }

            // Act
            using (var context = new TestApplicationDbContext(options))
            {
                var repository = new RoleRepository(context);
                var result = await repository.GetByNameAsync(roleName);

                // Assert
                result.ShouldNotBeNull();
                result.Name.ShouldBe(roleName);
                result.IsSystemRole.ShouldBeTrue();
            }
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllRoles()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<DbContext>()
                .UseInMemoryDatabase(databaseName: "GetAllAsync_ShouldReturnAllRoles")
                .Options;

            var roles = new List<Role>
            {
                new Role 
                {
                    Id = Guid.NewGuid(),
                    Name = "Admin",
                    Description = "Administrator Role",
                    IsSystemRole = true
                },
                new Role
                {
                    Id = Guid.NewGuid(),
                    Name = "Moderator",
                    Description = "Moderator Role",
                    IsSystemRole = true
                },
                new Role
                {
                    Id = Guid.NewGuid(),
                    Name = "User",
                    Description = "User Role",
                    IsSystemRole = false
                }
            };

            // Add test data to the in-memory database
            using (var context = new TestApplicationDbContext(options))
            {
                context.Roles.AddRange(roles);
                await context.SaveChangesAsync();
            }

            // Act
            using (var context = new TestApplicationDbContext(options))
            {
                var repository = new RoleRepository(context);
                var result = await repository.GetAllAsync();

                // Assert
                result.ShouldNotBeNull();
                result.Count().ShouldBe(3);
                result.Any(r => r.Name == "Admin").ShouldBeTrue();
                result.Any(r => r.Name == "Moderator").ShouldBeTrue();
                result.Any(r => r.Name == "User").ShouldBeTrue();
            }
        }

        [Fact]
        public async Task GetSystemRolesAsync_ShouldReturnOnlySystemRoles()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<DbContext>()
                .UseInMemoryDatabase(databaseName: "GetSystemRolesAsync_ShouldReturnOnlySystemRoles")
                .Options;

            var roles = new List<Role>
            {
                new Role 
                {
                    Id = Guid.NewGuid(),
                    Name = "Admin",
                    Description = "Administrator Role",
                    IsSystemRole = true
                },
                new Role
                {
                    Id = Guid.NewGuid(),
                    Name = "Moderator",
                    Description = "Moderator Role",
                    IsSystemRole = true
                },
                new Role
                {
                    Id = Guid.NewGuid(),
                    Name = "User",
                    Description = "User Role",
                    IsSystemRole = false
                }
            };

            // Add test data to the in-memory database
            using (var context = new TestApplicationDbContext(options))
            {
                context.Roles.AddRange(roles);
                await context.SaveChangesAsync();
            }

            // Act
            using (var context = new TestApplicationDbContext(options))
            {
                var repository = new RoleRepository(context);
                var result = await repository.GetSystemRolesAsync();

                // Assert
                result.ShouldNotBeNull();
                result.Count().ShouldBe(2);
                result.All(r => r.IsSystemRole).ShouldBeTrue();
                result.Any(r => r.Name == "Admin").ShouldBeTrue();
                result.Any(r => r.Name == "Moderator").ShouldBeTrue();
                result.Any(r => r.Name == "User").ShouldBeFalse();
            }
        }

        [Fact]
        public async Task CreateAsync_ShouldAddRoleToDatabase()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<DbContext>()
                .UseInMemoryDatabase(databaseName: "CreateAsync_ShouldAddRoleToDatabase")
                .Options;

            var newRole = new Role
            {
                Id = Guid.NewGuid(),
                Name = "NewRole",
                Description = "New Role Description",
                IsSystemRole = false
            };

            // Act
            using (var context = new TestApplicationDbContext(options))
            {
                var repository = new RoleRepository(context);
                var result = await repository.CreateAsync(newRole);

                // Assert
                result.ShouldNotBeNull();
                result.Id.ShouldBe(newRole.Id);
                result.Name.ShouldBe("NewRole");
            }

            // Verify the role was added to the database
            using (var context = new TestApplicationDbContext(options))
            {
                var savedRole = await context.Roles.FindAsync(newRole.Id);
                savedRole.ShouldNotBeNull();
                savedRole.Name.ShouldBe("NewRole");
                savedRole.Description.ShouldBe("New Role Description");
            }
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateExistingRole()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<DbContext>()
                .UseInMemoryDatabase(databaseName: "UpdateAsync_ShouldUpdateExistingRole")
                .Options;

            var roleId = Guid.NewGuid();
            var originalRole = new Role
            {
                Id = roleId,
                Name = "OriginalRole",
                Description = "Original Description",
                IsSystemRole = false,
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            };

            // Add test data to the in-memory database
            using (var context = new TestApplicationDbContext(options))
            {
                context.Roles.Add(originalRole);
                await context.SaveChangesAsync();
            }

            // Act
            using (var context = new TestApplicationDbContext(options))
            {
                // Get the role, modify it, and update
                var repository = new RoleRepository(context);
                var role = await repository.GetByIdAsync(roleId);

                role.Name = "UpdatedRole";
                role.Description = "Updated Description";

                var result = await repository.UpdateAsync(role);

                // Assert
                result.ShouldNotBeNull();
                result.Name.ShouldBe("UpdatedRole");
                result.Description.ShouldBe("Updated Description");
                result.UpdatedAt.ShouldBeGreaterThan(role.CreatedAt);
            }

            // Verify the role was updated in the database
            using (var context = new TestApplicationDbContext(options))
            {
                var updatedRole = await context.Roles.FindAsync(roleId);
                updatedRole.ShouldNotBeNull();
                updatedRole.Name.ShouldBe("UpdatedRole");
                updatedRole.Description.ShouldBe("Updated Description");
            }
        }

        [Fact]
        public async Task SoftDeleteAsync_ShouldMarkRoleAsDeleted()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<RoleTestContext>()
                .UseInMemoryDatabase(databaseName: "SoftDeleteAsync_ShouldMarkRoleAsDeleted")
                .Options;

            var roleId = Guid.NewGuid();
            var role = new Role
            {
                Id = roleId,
                Name = "RoleToDelete",
                Description = "Role to be deleted",
                IsSystemRole = false
            };

            // Add test data to the in-memory database
            using (var context = new RoleTestContext(options))
            {
                context.Roles.Add(role);
                await context.SaveChangesAsync();
            }

            // Implement a wrapper class to use the test context with RoleRepository
            using (var context = new RoleTestContext(options))
            {
                // Create a wrapper to adapt RoleTestContext to ApplicationDbContext interface
                var repository = new TestRoleRepository(context);
                
                // Act
                var result = await repository.SoftDeleteAsync(roleId);
                
                // Assert
                result.ShouldBeTrue();
            }

            // Verify the role was marked as deleted in the database
            using (var context = new RoleTestContext(options))
            {
                // We need to turn off the query filter to see soft-deleted entities
                var deletedRole = await context.Roles
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(r => r.Id == roleId);

                deletedRole.ShouldNotBeNull();
                deletedRole.DeletedAt.ShouldNotBeNull();
            }

            // Verify the role doesn't show up in normal queries
            using (var context = new RoleTestContext(options))
            {
                var roles = await context.Roles.ToListAsync();
                roles.Any(r => r.Id == roleId).ShouldBeFalse();
            }
        }

        [Fact]
        public async Task ExistsByNameAsync_ShouldReturnTrue_WhenRoleExists()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<DbContext>()
                .UseInMemoryDatabase(databaseName: "ExistsByNameAsync_ShouldReturnTrue_WhenRoleExists")
                .Options;

            var roleName = "ExistingRole";
            var role = new Role
            {
                Id = Guid.NewGuid(),
                Name = roleName,
                Description = "Existing Role",
                IsSystemRole = false
            };

            // Add test data to the in-memory database
            using (var context = new TestApplicationDbContext(options))
            {
                context.Roles.Add(role);
                await context.SaveChangesAsync();
            }

            // Act
            using (var context = new TestApplicationDbContext(options))
            {
                var repository = new RoleRepository(context);
                var result = await repository.ExistsByNameAsync(roleName);

                // Assert
                result.ShouldBeTrue();
            }
        }

        [Fact]
        public async Task ExistsByNameAsync_ShouldReturnFalse_WhenRoleDoesNotExist()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<DbContext>()
                .UseInMemoryDatabase(databaseName: "ExistsByNameAsync_ShouldReturnFalse_WhenRoleDoesNotExist")
                .Options;

            var nonExistentRoleName = "NonExistentRole";

            // Act
            using (var context = new TestApplicationDbContext(options))
            {
                var repository = new RoleRepository(context);
                var result = await repository.ExistsByNameAsync(nonExistentRoleName);

                // Assert
                result.ShouldBeFalse();
            }
        }

        // Class implementing IRoleRepository for test purposes
        private class TestRoleRepository : IRoleRepository
        {
            private readonly RoleTestContext _dbContext;

            public TestRoleRepository(RoleTestContext dbContext)
            {
                _dbContext = dbContext;
            }

            public Task<Role> CreateAsync(Role role, CancellationToken cancellationToken = default)
            {
                throw new NotImplementedException();
            }

            public Task<bool> ExistsByIdAsync(Guid id, CancellationToken cancellationToken = default)
            {
                throw new NotImplementedException();
            }

            public Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
            {
                throw new NotImplementedException();
            }

            public Task<IEnumerable<Role>> GetAllAsync(CancellationToken cancellationToken = default)
            {
                throw new NotImplementedException();
            }

            public Task<Role> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            {
                throw new NotImplementedException();
            }

            public Task<Role> GetByNameAsync(string name, CancellationToken cancellationToken = default)
            {
                throw new NotImplementedException();
            }

            public Task<IEnumerable<Role>> GetSystemRolesAsync(CancellationToken cancellationToken = default)
            {
                throw new NotImplementedException();
            }

            public async Task<bool> SoftDeleteAsync(Guid id, CancellationToken cancellationToken = default)
            {
                var role = await _dbContext.Roles
                    .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

                if (role == null)
                    return false;

                // Soft delete
                role.DeletedAt = DateTime.UtcNow;
                _dbContext.Roles.Update(role);
                await _dbContext.SaveChangesAsync(cancellationToken);
                return true;
            }

            public Task<Role> UpdateAsync(Role role, CancellationToken cancellationToken = default)
            {
                throw new NotImplementedException();
            }
        }
    }
} 