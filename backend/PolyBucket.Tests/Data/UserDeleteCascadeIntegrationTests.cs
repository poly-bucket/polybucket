using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Features.Authentication.Domain;
using PolyBucket.Api.Features.Users.Domain;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Data;

[Collection("TestCollection")]
public class UserDeleteCascadeIntegrationTests : BaseIntegrationTest
{
    public UserDeleteCascadeIntegrationTests(TestCollectionFixture testFixture) : base(testFixture)
    {
    }

    [Fact(DisplayName = "When a user is deleted, dependent rows cascade or null FKs without manual UserLogins cleanup.")]
    public async Task DeleteUser_CascadesAndSetNulls()
    {
        // Arrange
        await ResetStateAsync();
        var user = await UserFactory.CreateTestUser();
        var loginId = Guid.NewGuid();
        DbContext.UserLogins.Add(new UserLogin
        {
            Id = loginId,
            Email = user.Email,
            UserAgent = "test",
            CreatedAt = DateTime.UtcNow,
            UserId = user.Id,
            Successful = true
        });
        DbContext.UserSettings.Add(new UserSettings
        {
            Id = Guid.NewGuid(),
            UserId = user.Id
        });
        DbContext.RefreshTokens.Add(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = Guid.NewGuid().ToString("N"),
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedByIp = "127.0.0.1"
        });
        DbContext.UserAuditLogs.Add(new UserAuditLog
        {
            UserId = user.Id,
            Action = UserAuditAction.EmailMarkedVerified,
            CreatedAt = DateTime.UtcNow
        });
        await ModelFactory.CreateTestModel(userId: user.Id);
        await DbContext.SaveChangesAsync();

        // Act
        DbContext.Users.Remove(user);
        await DbContext.SaveChangesAsync();

        // Assert
        (await DbContext.Users.AnyAsync(u => u.Id == user.Id)).ShouldBeFalse();
        (await DbContext.UserSettings.AnyAsync(s => s.UserId == user.Id)).ShouldBeFalse();
        (await DbContext.RefreshTokens.AnyAsync(t => t.UserId == user.Id)).ShouldBeFalse();
        (await DbContext.UserAuditLogs.AnyAsync(a => a.UserId == user.Id)).ShouldBeFalse();
        (await DbContext.Models.AnyAsync(m => m.AuthorId == user.Id)).ShouldBeFalse();

        var login = await DbContext.UserLogins.AsNoTracking().FirstAsync(l => l.Id == loginId);
        login.UserId.ShouldBeNull();
    }

    [Fact(DisplayName = "When a user who banned others is deleted, BannedByUserId on banned users is set null.")]
    public async Task DeleteBanner_SetsBannedByUserIdNull()
    {
        // Arrange
        await ResetStateAsync();
        var moderator = await UserFactory.CreateTestUser();
        var banned = await UserFactory.CreateTestUser();
        banned.IsBanned = true;
        banned.BannedAt = DateTime.UtcNow;
        banned.BannedByUserId = moderator.Id;
        await DbContext.SaveChangesAsync();

        // Act
        DbContext.Users.Remove(moderator);
        await DbContext.SaveChangesAsync();

        // Assert
        var reloaded = await DbContext.Users.AsNoTracking().SingleAsync(u => u.Id == banned.Id);
        reloaded.BannedByUserId.ShouldBeNull();
        reloaded.IsBanned.ShouldBeTrue();
    }
}
