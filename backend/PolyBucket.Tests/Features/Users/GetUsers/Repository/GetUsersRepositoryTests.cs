using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.Authentication.Domain;
using PolyBucket.Api.Features.Users.GetUsers.Domain;
using PolyBucket.Api.Features.Users.GetUsers.Repository;
using Xunit;

namespace PolyBucket.Tests.Features.Users.GetUsers.Repository;

public class GetUsersRepositoryTests
{
    private readonly DbContextOptions<PolyBucketDbContext> _options;

    public GetUsersRepositoryTests()
    {
        _options = new DbContextOptionsBuilder<PolyBucketDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    private static PolyBucketDbContext CreateContext(DbContextOptions<PolyBucketDbContext> options) => new(options);

    private static async Task SeedTestData(PolyBucketDbContext context)
    {
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

        var t = DateTime.UtcNow;
        const string testSalt = "test-salt";
        const string testPasswordHash = "test-password-hash";
        var users = new List<User>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Email = "admin@example.com",
                Username = "admin",
                FirstName = "Admin",
                LastName = "User",
                Salt = testSalt,
                PasswordHash = testPasswordHash,
                RoleId = adminRole.Id,
                IsBanned = false,
                LastLoginAt = t.AddHours(-2),
                CreatedAt = t.AddDays(-10),
                UpdatedAt = t.AddDays(-10)
            },
            new()
            {
                Id = Guid.NewGuid(),
                Email = "user1@example.com",
                Username = "user1",
                FirstName = "John",
                LastName = "Doe",
                Salt = testSalt,
                PasswordHash = testPasswordHash,
                RoleId = userRole.Id,
                IsBanned = false,
                LastLoginAt = t.AddHours(-5),
                CreatedAt = t.AddDays(-5),
                UpdatedAt = t.AddDays(-5)
            },
            new()
            {
                Id = Guid.NewGuid(),
                Email = "user2@example.com",
                Username = "user2",
                FirstName = "Jane",
                LastName = "Smith",
                Salt = testSalt,
                PasswordHash = testPasswordHash,
                RoleId = userRole.Id,
                IsBanned = true,
                BannedAt = t.AddDays(-2),
                BanReason = "Violation of terms",
                CreatedAt = t.AddDays(-8),
                UpdatedAt = t.AddDays(-2)
            },
            new()
            {
                Id = Guid.NewGuid(),
                Email = "moderator@example.com",
                Username = "moderator",
                FirstName = "Mod",
                LastName = "User",
                Salt = testSalt,
                PasswordHash = testPasswordHash,
                RoleId = moderatorRole.Id,
                IsBanned = false,
                LastLoginAt = t.AddHours(-1),
                CreatedAt = t.AddDays(-3),
                UpdatedAt = t.AddDays(-3)
            }
        };

        context.Users.AddRange(users);

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

        var logins = new List<UserLogin>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Email = "admin@example.com",
                UserId = users[0].Id,
                Successful = true,
                UserAgent = "Test Browser",
                CreatedAt = t.AddHours(-2)
            },
            new()
            {
                Id = Guid.NewGuid(),
                Email = "user1@example.com",
                UserId = users[1].Id,
                Successful = true,
                UserAgent = "Test Browser",
                CreatedAt = t.AddHours(-5)
            },
            new()
            {
                Id = Guid.NewGuid(),
                Email = "moderator@example.com",
                UserId = users[3].Id,
                Successful = true,
                UserAgent = "Test Browser",
                CreatedAt = t.AddHours(-1)
            }
        };

        context.UserLogins.AddRange(logins);

        await context.SaveChangesAsync();
    }

    [Fact(DisplayName = "When getting users with the default query, the get users repository returns all users.")]
    public async Task GetPagedAsync_WithDefaultQuery_ReturnsAllUsers()
    {
        await using var context = CreateContext(_options);
        await SeedTestData(context);
        var repository = new GetUsersRepository(context);
        var query = new GetUsersQuery();

        var result = await repository.GetPagedAsync(query, CancellationToken.None);

        Assert.Equal(4, result.TotalCount);
        Assert.Equal(4, result.Users.Count());
        Assert.Equal(1, result.Page);
        Assert.Equal(20, result.PageSize);
    }

    [Fact(DisplayName = "When getting users with pagination, the get users repository returns the requested page.")]
    public async Task GetPagedAsync_WithPagination_ReturnsCorrectPage()
    {
        await using var context = CreateContext(_options);
        await SeedTestData(context);
        var repository = new GetUsersRepository(context);
        var query = new GetUsersQuery { Page = 1, PageSize = 2 };

        var result = await repository.GetPagedAsync(query, CancellationToken.None);

        Assert.Equal(4, result.TotalCount);
        Assert.Equal(2, result.Users.Count());
        Assert.Equal(2, result.TotalPages);
    }

    [Fact(DisplayName = "When getting users with a search query, the get users repository filters the results to matching users.")]
    public async Task GetPagedAsync_WithSearchQuery_FiltersUsersCorrectly()
    {
        await using var context = CreateContext(_options);
        await SeedTestData(context);
        var repository = new GetUsersRepository(context);
        var query = new GetUsersQuery { SearchQuery = "admin" };

        var result = await repository.GetPagedAsync(query, CancellationToken.None);

        Assert.Equal(1, result.TotalCount);
        Assert.Contains(result.Users, u => u.Username == "admin");
    }

    [Fact(DisplayName = "When getting users with a role filter, the get users repository returns only users with that role.")]
    public async Task GetPagedAsync_WithRoleFilter_FiltersUsersByRole()
    {
        await using var context = CreateContext(_options);
        await SeedTestData(context);
        var repository = new GetUsersRepository(context);
        var query = new GetUsersQuery { RoleFilter = "User" };

        var result = await repository.GetPagedAsync(query, CancellationToken.None);

        Assert.Equal(2, result.TotalCount);
        Assert.All(result.Users, u => Assert.Equal("User", u.RoleName));
    }

    [Fact(DisplayName = "When getting users with a status filter, the get users repository returns only users matching that status.")]
    public async Task GetPagedAsync_WithStatusFilter_FiltersUsersByStatus()
    {
        await using var context = CreateContext(_options);
        await SeedTestData(context);
        var repository = new GetUsersRepository(context);
        var query = new GetUsersQuery { StatusFilter = "Banned" };

        var result = await repository.GetPagedAsync(query, CancellationToken.None);

        Assert.Equal(1, result.TotalCount);
        Assert.All(result.Users, u => Assert.True(u.IsBanned));
    }

    [Fact(DisplayName = "When getting users with a username sort, the get users repository returns users in the requested order.")]
    public async Task GetPagedAsync_WithSorting_SortsUsersCorrectly()
    {
        await using var context = CreateContext(_options);
        await SeedTestData(context);
        var repository = new GetUsersRepository(context);
        var query = new GetUsersQuery { SortBy = "Username", SortDescending = false };

        var result = await repository.GetPagedAsync(query, CancellationToken.None);

        var users = result.Users.ToList();
        Assert.Equal("admin", users[0].Username);
        Assert.Equal("moderator", users[1].Username);
        Assert.Equal("user1", users[2].Username);
        Assert.Equal("user2", users[3].Username);
    }

    [Fact(DisplayName = "When getting users sorted by last login, the get users repository orders users by their last login time.")]
    public async Task GetPagedAsync_WithLastLoginSorting_SortsByLastLogin()
    {
        await using var context = CreateContext(_options);
        await SeedTestData(context);
        var repository = new GetUsersRepository(context);
        var query = new GetUsersQuery { SortBy = "LastLogin", SortDescending = true };

        var result = await repository.GetPagedAsync(query, CancellationToken.None);

        var users = result.Users.ToList();
        Assert.Equal("moderator", users[0].Username);
        Assert.Equal("admin", users[1].Username);
        Assert.Equal("user1", users[2].Username);
        Assert.Equal("user2", users[3].Username);
    }

    [Fact(DisplayName = "When getting users with combined filters and sorting, the get users repository applies all of them together.")]
    public async Task GetPagedAsync_WithComplexFilters_AppliesAllFilters()
    {
        await using var context = CreateContext(_options);
        await SeedTestData(context);
        var repository = new GetUsersRepository(context);
        var query = new GetUsersQuery
        {
            SearchQuery = "user",
            RoleFilter = "User",
            StatusFilter = "Active",
            SortBy = "CreatedAt",
            SortDescending = true
        };

        var result = await repository.GetPagedAsync(query, CancellationToken.None);

        Assert.Equal(1, result.TotalCount);
        Assert.Contains(result.Users, u => u.Username == "user1" && !u.IsBanned);
    }

    [Fact(DisplayName = "When getting users and no records match, the get users repository returns an empty list.")]
    public async Task GetPagedAsync_WithNoResults_ReturnsEmptyList()
    {
        await using var context = CreateContext(_options);
        await SeedTestData(context);
        var repository = new GetUsersRepository(context);
        var query = new GetUsersQuery { SearchQuery = "nonexistent" };

        var result = await repository.GetPagedAsync(query, CancellationToken.None);

        Assert.Equal(0, result.TotalCount);
        Assert.Empty(result.Users);
        Assert.Equal(0, result.TotalPages);
    }
}
