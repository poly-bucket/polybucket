using Microsoft.EntityFrameworkCore;
using PolyBucket.Database.Entities.ACL;
using PolyBucket.Database.Entities.Models;
using PolyBucket.Database.Entities.Models.Enums;
using PolyBucket.Database.Persistence;

namespace PolyBucket.Database.Tests;

[Collection("DatabaseTestCollection")]
public abstract class DatabaseIntegrationTestBase : IAsyncLifetime
{
    protected PolyBucketDbContext DbContext { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        DbContext = DatabaseTestManager.CreateContext();
        await ResetDatabaseAsync();
    }

    public async Task DisposeAsync()
    {
        await DbContext.DisposeAsync();
    }

    protected async Task ResetDatabaseAsync()
    {
        await DbContext.Database.EnsureDeletedAsync();
        await DbContext.Database.MigrateAsync();
    }

    protected async Task<User> SeedUserAsync(string? email = null)
    {
        var role = await DbContext.Roles.FirstOrDefaultAsync(r => r.Name == "User");
        if (role is null)
        {
            role = new Role
            {
                Id = Guid.NewGuid(),
                Name = "User",
                Description = "Standard user role",
                IsActive = true,
                IsDefault = true,
                IsSystemRole = true,
                CanBeDeleted = false,
                Priority = 100
            };
            DbContext.Roles.Add(role);
            await DbContext.SaveChangesAsync();
        }

        var salt = BCrypt.Net.BCrypt.GenerateSalt();
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email ?? $"user_{Guid.NewGuid():N}@example.com",
            Username = $"user_{Guid.NewGuid():N}"[..20],
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("TestPassword123!", salt),
            Salt = salt,
            RoleId = role.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        DbContext.Users.Add(user);
        await DbContext.SaveChangesAsync();
        return user;
    }

    protected async Task<Model> SeedModelAsync(Guid authorId)
    {
        var model = new Model
        {
            Id = Guid.NewGuid(),
            Name = "Test Model",
            Description = "Test",
            AuthorId = authorId,
            Privacy = PrivacySettings.Public,
            IsPublic = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        DbContext.Models.Add(model);
        await DbContext.SaveChangesAsync();
        return model;
    }
}
