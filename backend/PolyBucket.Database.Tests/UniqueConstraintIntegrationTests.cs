using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Database.Entities.Authentication;
using PolyBucket.Database.Entities.Models;
using PolyBucket.Database.Entities.Users;
using Shouldly;
using Xunit;

namespace PolyBucket.Database.Tests;

[Collection("DatabaseTestCollection")]
public class UniqueConstraintIntegrationTests : DatabaseIntegrationTestBase
{

    [Fact(DisplayName = "When two users share the same email, the unique index rejects the insert.")]
    public async Task DuplicateEmail_IsRejected()
    {
        // Arrange
        await ResetDatabaseAsync();
        var first = await SeedUserAsync("dup@example.com");
        var salt = BCrypt.Net.BCrypt.GenerateSalt();
        var second = new PolyBucket.Database.Entities.Models.User
        {
            Id = Guid.NewGuid(),
            Email = first.Email,
            Username = "other_dup_user",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("TestPassword123!", salt),
            Salt = salt,
            RoleId = first.RoleId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        DbContext.Users.Add(second);
        var act = () => DbContext.SaveChangesAsync();

        // Assert
        await Should.ThrowAsync<DbUpdateException>(act);
    }

    [Fact(DisplayName = "When two users share the same username, the unique index rejects the insert.")]
    public async Task DuplicateUsername_IsRejected()
    {
        // Arrange
        await ResetDatabaseAsync();
        var first = await SeedUserAsync();
        var salt = BCrypt.Net.BCrypt.GenerateSalt();
        var second = new PolyBucket.Database.Entities.Models.User
        {
            Id = Guid.NewGuid(),
            Email = "unique_email_for_username_dup@example.com",
            Username = first.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("TestPassword123!", salt),
            Salt = salt,
            RoleId = first.RoleId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        DbContext.Users.Add(second);
        var act = () => DbContext.SaveChangesAsync();

        // Assert
        await Should.ThrowAsync<DbUpdateException>(act);
    }

    [Fact(DisplayName = "When two refresh tokens share the same token string, the unique index rejects the insert.")]
    public async Task DuplicateRefreshToken_IsRejected()
    {
        // Arrange
        await ResetDatabaseAsync();
        var user = await SeedUserAsync();
        var token = Guid.NewGuid().ToString("N");
        DbContext.RefreshTokens.Add(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = token,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            CreatedByIp = "127.0.0.1"
        });
        await DbContext.SaveChangesAsync();
        DbContext.RefreshTokens.Add(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = token,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            CreatedByIp = "127.0.0.1"
        });

        // Act
        var act = () => DbContext.SaveChangesAsync();

        // Assert
        await Should.ThrowAsync<DbUpdateException>(act);
    }

    [Fact(DisplayName = "When two active likes exist for the same model and user, the partial unique index rejects the insert.")]
    public async Task DuplicateActiveLike_IsRejected()
    {
        // Arrange
        await ResetDatabaseAsync();
        var user = await SeedUserAsync();
        var model = await SeedModelAsync(user.Id);
        DbContext.Likes.Add(new Like
        {
            Id = Guid.NewGuid(),
            ModelId = model.Id,
            UserId = user.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await DbContext.SaveChangesAsync();
        DbContext.Likes.Add(new Like
        {
            Id = Guid.NewGuid(),
            ModelId = model.Id,
            UserId = user.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        // Act
        var act = () => DbContext.SaveChangesAsync();

        // Assert
        await Should.ThrowAsync<DbUpdateException>(act);
    }

    [Fact(DisplayName = "When a like is soft-deleted, a new active like for the same model and user is allowed.")]
    public async Task SoftDeletedLike_AllowsNewActiveLike()
    {
        // Arrange
        await ResetDatabaseAsync();
        var user = await SeedUserAsync();
        var model = await SeedModelAsync(user.Id);
        var oldLikeId = Guid.NewGuid();
        DbContext.Likes.Add(new Like
        {
            Id = oldLikeId,
            ModelId = model.Id,
            UserId = user.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            DeletedAt = DateTime.UtcNow
        });
        await DbContext.SaveChangesAsync();
        DbContext.Likes.Add(new Like
        {
            Id = Guid.NewGuid(),
            ModelId = model.Id,
            UserId = user.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        // Act
        await DbContext.SaveChangesAsync();

        // Assert
        (await DbContext.Likes.CountAsync(l => l.ModelId == model.Id && l.UserId == user.Id && l.DeletedAt == null)).ShouldBe(1);
    }

    [Fact(DisplayName = "When two external auth rows share provider and external id, the unique index rejects the insert.")]
    public async Task DuplicateExternalAuth_IsRejected()
    {
        // Arrange
        await ResetDatabaseAsync();
        var user = await SeedUserAsync();
        var other = await SeedUserAsync();
        DbContext.ExternalAuthProviders.Add(new ExternalAuthProvider
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Provider = "GitHub",
            ExternalId = "ext-1",
            Email = user.Email,
            LastLoginAt = DateTime.UtcNow
        });
        await DbContext.SaveChangesAsync();
        DbContext.ExternalAuthProviders.Add(new ExternalAuthProvider
        {
            Id = Guid.NewGuid(),
            UserId = other.Id,
            Provider = "GitHub",
            ExternalId = "ext-1",
            Email = other.Email,
            LastLoginAt = DateTime.UtcNow
        });

        // Act
        var act = () => DbContext.SaveChangesAsync();

        // Assert
        await Should.ThrowAsync<DbUpdateException>(act);
    }
}
