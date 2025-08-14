using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.Authentication.Domain;
using PolyBucket.Api.Features.Users.Domain;
using PolyBucket.Api.Features.Users.Queries.GetUsers;
using PolyBucket.Api.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace PolyBucket.Tests.Features.Users.Queries.GetUsers
{
    public class GetUsersQueryHandlerTests
    {
        private readonly DbContextOptions<PolyBucketDbContext> _options;
        private readonly Mock<ILogger<GetUsersQueryHandler>> _mockLogger;

        public GetUsersQueryHandlerTests()
        {
            _options = new DbContextOptionsBuilder<PolyBucketDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _mockLogger = new Mock<ILogger<GetUsersQueryHandler>>();
        }

        private PolyBucketDbContext CreateContext()
        {
            return new PolyBucketDbContext(_options);
        }

        private async Task SeedTestData(PolyBucketDbContext context)
        {
            // Create roles
            var adminRole = new Role
            {
                Id = Guid.NewGuid(),
                Name = "Admin",
                Description = "Administrator role",
                Priority = 1000,
                IsSystemRole = true,
                IsActive = true
            };

            var userRole = new Role
            {
                Id = Guid.NewGuid(),
                Name = "User",
                Description = "Standard user role",
                Priority = 100,
                IsSystemRole = true,
                IsActive = true
            };

            var moderatorRole = new Role
            {
                Id = Guid.NewGuid(),
                Name = "Moderator",
                Description = "Moderator role",
                Priority = 500,
                IsSystemRole = true,
                IsActive = true
            };

            context.Roles.AddRange(adminRole, userRole, moderatorRole);

            // Create users
            var users = new List<User>
            {
                new User
                {
                    Id = Guid.NewGuid(),
                    Email = "admin@example.com",
                    Username = "admin",
                    FirstName = "Admin",
                    LastName = "User",
                    RoleId = adminRole.Id,
                    IsBanned = false,
                    CreatedAt = DateTime.UtcNow.AddDays(-10),
                    UpdatedAt = DateTime.UtcNow.AddDays(-10)
                },
                new User
                {
                    Id = Guid.NewGuid(),
                    Email = "user1@example.com",
                    Username = "user1",
                    FirstName = "John",
                    LastName = "Doe",
                    RoleId = userRole.Id,
                    IsBanned = false,
                    CreatedAt = DateTime.UtcNow.AddDays(-5),
                    UpdatedAt = DateTime.UtcNow.AddDays(-5)
                },
                new User
                {
                    Id = Guid.NewGuid(),
                    Email = "user2@example.com",
                    Username = "user2",
                    FirstName = "Jane",
                    LastName = "Smith",
                    RoleId = userRole.Id,
                    IsBanned = true,
                    BannedAt = DateTime.UtcNow.AddDays(-2),
                    BanReason = "Violation of terms",
                    CreatedAt = DateTime.UtcNow.AddDays(-8),
                    UpdatedAt = DateTime.UtcNow.AddDays(-2)
                },
                new User
                {
                    Id = Guid.NewGuid(),
                    Email = "moderator@example.com",
                    Username = "moderator",
                    FirstName = "Mod",
                    LastName = "User",
                    RoleId = moderatorRole.Id,
                    IsBanned = false,
                    CreatedAt = DateTime.UtcNow.AddDays(-3),
                    UpdatedAt = DateTime.UtcNow.AddDays(-3)
                }
            };

            context.Users.AddRange(users);

            // Create user settings
            var userSettings = users.Select(u => new PolyBucket.Api.Features.Users.Domain.UserSettings
            {
                Id = Guid.NewGuid(),
                UserId = u.Id,
                Language = "en",
                Theme = "dark",
                EmailNotifications = true,
                MeasurementSystem = "metric",
                TimeZone = "UTC"
            });

            context.UserSettings.AddRange(userSettings);

            // Create login records
            var logins = new List<UserLogin>
            {
                new UserLogin
                {
                    Id = Guid.NewGuid(),
                    Email = "admin@example.com",
                    UserId = users[0].Id,
                    Successful = true,
                    UserAgent = "Test Browser",
                    CreatedAt = DateTime.UtcNow.AddHours(-2)
                },
                new UserLogin
                {
                    Id = Guid.NewGuid(),
                    Email = "user1@example.com",
                    UserId = users[1].Id,
                    Successful = true,
                    UserAgent = "Test Browser",
                    CreatedAt = DateTime.UtcNow.AddHours(-5)
                },
                new UserLogin
                {
                    Id = Guid.NewGuid(),
                    Email = "moderator@example.com",
                    UserId = users[3].Id,
                    Successful = true,
                    UserAgent = "Test Browser",
                    CreatedAt = DateTime.UtcNow.AddHours(-1)
                }
            };

            context.UserLogins.AddRange(logins);

            await context.SaveChangesAsync();
        }

        [Fact]
        public async Task Handle_WithDefaultQuery_ReturnsAllUsers()
        {
            // Arrange
            using var context = CreateContext();
            await SeedTestData(context);
            var handler = new GetUsersQueryHandler(context, _mockLogger.Object);
            var query = new GetUsersQuery();

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Equal(4, result.TotalCount);
            Assert.Equal(4, result.Users.Count());
            Assert.Equal(1, result.Page);
            Assert.Equal(20, result.PageSize);
        }

        [Fact]
        public async Task Handle_WithPagination_ReturnsCorrectPage()
        {
            // Arrange
            using var context = CreateContext();
            await SeedTestData(context);
            var handler = new GetUsersQueryHandler(context, _mockLogger.Object);
            var query = new GetUsersQuery { Page = 1, PageSize = 2 };

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Equal(4, result.TotalCount);
            Assert.Equal(2, result.Users.Count());
            Assert.Equal(2, result.TotalPages);
        }

        [Fact]
        public async Task Handle_WithSearchQuery_FiltersUsersCorrectly()
        {
            // Arrange
            using var context = CreateContext();
            await SeedTestData(context);
            var handler = new GetUsersQueryHandler(context, _mockLogger.Object);
            var query = new GetUsersQuery { SearchQuery = "admin" };

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Equal(1, result.TotalCount);
            Assert.Contains(result.Users, u => u.Username == "admin");
        }

        [Fact]
        public async Task Handle_WithRoleFilter_FiltersUsersByRole()
        {
            // Arrange
            using var context = CreateContext();
            await SeedTestData(context);
            var handler = new GetUsersQueryHandler(context, _mockLogger.Object);
            var query = new GetUsersQuery { RoleFilter = "User" };

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Equal(2, result.TotalCount);
            Assert.All(result.Users, u => Assert.Equal("User", u.RoleName));
        }

        [Fact]
        public async Task Handle_WithStatusFilter_FiltersUsersByStatus()
        {
            // Arrange
            using var context = CreateContext();
            await SeedTestData(context);
            var handler = new GetUsersQueryHandler(context, _mockLogger.Object);
            var query = new GetUsersQuery { StatusFilter = "Banned" };

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Equal(1, result.TotalCount);
            Assert.All(result.Users, u => Assert.True(u.IsBanned));
        }

        [Fact]
        public async Task Handle_WithSorting_SortsUsersCorrectly()
        {
            // Arrange
            using var context = CreateContext();
            await SeedTestData(context);
            var handler = new GetUsersQueryHandler(context, _mockLogger.Object);
            var query = new GetUsersQuery { SortBy = "Username", SortDescending = false };

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            var users = result.Users.ToList();
            Assert.Equal("admin", users[0].Username);
            Assert.Equal("moderator", users[1].Username);
            Assert.Equal("user1", users[2].Username);
            Assert.Equal("user2", users[3].Username);
        }

        [Fact]
        public async Task Handle_WithLastLoginSorting_SortsByLastLogin()
        {
            // Arrange
            using var context = CreateContext();
            await SeedTestData(context);
            var handler = new GetUsersQueryHandler(context, _mockLogger.Object);
            var query = new GetUsersQuery { SortBy = "LastLogin", SortDescending = true };

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            var users = result.Users.ToList();
            // moderator logged in most recently (1 hour ago)
            Assert.Equal("moderator", users[0].Username);
            // admin logged in 2 hours ago
            Assert.Equal("admin", users[1].Username);
            // user1 logged in 5 hours ago
            Assert.Equal("user1", users[2].Username);
            // user2 has no login record
            Assert.Equal("user2", users[3].Username);
        }

        [Fact]
        public async Task Handle_WithComplexFilters_AppliesAllFilters()
        {
            // Arrange
            using var context = CreateContext();
            await SeedTestData(context);
            var handler = new GetUsersQueryHandler(context, _mockLogger.Object);
            var query = new GetUsersQuery
            {
                SearchQuery = "user",
                RoleFilter = "User",
                StatusFilter = "Active",
                SortBy = "CreatedAt",
                SortDescending = true
            };

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Equal(1, result.TotalCount);
            Assert.Contains(result.Users, u => u.Username == "user1" && !u.IsBanned);
        }

        [Fact]
        public async Task Handle_WithNoResults_ReturnsEmptyList()
        {
            // Arrange
            using var context = CreateContext();
            await SeedTestData(context);
            var handler = new GetUsersQueryHandler(context, _mockLogger.Object);
            var query = new GetUsersQuery { SearchQuery = "nonexistent" };

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Equal(0, result.TotalCount);
            Assert.Empty(result.Users);
            Assert.Equal(0, result.TotalPages);
        }

        [Fact]
        public async Task Handle_WithException_ThrowsException()
        {
            // Arrange
            var mockContext = new Mock<PolyBucketDbContext>();
            mockContext.Setup(c => c.Users).Throws(new Exception("Database connection failed"));
            var handler = new GetUsersQueryHandler(mockContext.Object, _mockLogger.Object);
            var query = new GetUsersQuery();

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => handler.Handle(query, CancellationToken.None));
        }
    }
}
