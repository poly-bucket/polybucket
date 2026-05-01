using System;
using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Features.Authentication.Domain;
using PolyBucket.Api.Features.Authentication.Account.Domain;
using PolyBucket.Api.Features.Authentication.Account.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Authentication.Http;

[Collection("TestCollection")]
public class DangerZoneAccountControllerTests : BaseIntegrationTest
{
    public DangerZoneAccountControllerTests(TestCollectionFixture testFixture) : base(testFixture)
    {
    }

    [Fact(DisplayName = "GET export returns profile JSON when authenticated.")]
    public async Task ExportAccount_WithAuth_ReturnsOk()
    {
        // Arrange
        await ResetStateAsync();
        var user = await CreateTestUser();
        var token = await GetAuthToken(user.Email, "TestPassword123!");
        token.ShouldNotBeNullOrEmpty();

        SetAuthHeaders(token);

        // Act
        var response = await Client.GetAsync("/api/auth/account/export");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<AccountExportResponse>();
        body.ShouldNotBeNull();
        body!.Email.ShouldBe(user.Email);
        body.Username.ShouldBe(user.Username);
        body.ModelCount.ShouldBeGreaterThanOrEqualTo(0);
        body.CollectionCount.ShouldBeGreaterThanOrEqualTo(0);
        body.RecentModels.ShouldNotBeNull();
        body.RecentCollections.ShouldNotBeNull();
    }

    [Fact(DisplayName = "POST sessions/revoke-all returns success when authenticated.")]
    public async Task RevokeAllSessions_WithAuth_ReturnsOk()
    {
        // Arrange
        await ResetStateAsync();
        var user = await CreateTestUser();
        var token = await GetAuthToken(user.Email, "TestPassword123!");
        SetAuthHeaders(token);

        // Act
        var response = await Client.PostAsync("/api/auth/account/sessions/revoke-all", null);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<RevokeAllSessionsResponse>();
        body?.Success.ShouldBeTrue();
    }

    [Fact(DisplayName = "GET sessions returns active sessions for authenticated user.")]
    public async Task GetActiveSessions_WithAuth_ReturnsSessions()
    {
        // Arrange
        await ResetStateAsync();
        var user = await CreateTestUser();
        var token = await GetAuthToken(user.Email, "TestPassword123!");
        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = $"session-token-{Guid.NewGuid():N}",
            UserId = user.Id,
            CreatedAt = DateTime.UtcNow.AddMinutes(-10),
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedByIp = "10.0.0.1",
        };
        DbContext.RefreshTokens.Add(refreshToken);
        await DbContext.SaveChangesAsync();
        SetAuthHeaders(token);

        // Act
        var response = await Client.GetAsync("/api/auth/account/sessions");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<GetActiveSessionsResponse>();
        body.ShouldNotBeNull();
        body!.Sessions.Count.ShouldBeGreaterThanOrEqualTo(1);
        body.Sessions.ShouldContain(s => s.SessionId == refreshToken.Id);
    }

    [Fact(DisplayName = "POST session revoke endpoint revokes selected session.")]
    public async Task RevokeOneSession_WithAuth_RevokesSession()
    {
        // Arrange
        await ResetStateAsync();
        var user = await CreateTestUser();
        var token = await GetAuthToken(user.Email, "TestPassword123!");
        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = $"session-token-{Guid.NewGuid():N}",
            UserId = user.Id,
            CreatedAt = DateTime.UtcNow.AddMinutes(-10),
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedByIp = "10.0.0.1",
        };
        DbContext.RefreshTokens.Add(refreshToken);
        await DbContext.SaveChangesAsync();
        SetAuthHeaders(token);

        // Act
        var response = await Client.PostAsync($"/api/auth/account/sessions/{refreshToken.Id}/revoke", null);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<RevokeSessionResponse>();
        body.ShouldNotBeNull();
        body!.Success.ShouldBeTrue();

        DbContext.ChangeTracker.Clear();
        var revokedToken = await DbContext.RefreshTokens.AsNoTracking().FirstOrDefaultAsync(rt => rt.Id == refreshToken.Id);
        revokedToken.ShouldNotBeNull();
        revokedToken!.RevokedAt.ShouldNotBeNull();
    }

    [Fact(DisplayName = "POST account/delete with wrong password returns BadRequest.")]
    public async Task DeleteAccount_WithWrongPassword_ReturnsBadRequest()
    {
        // Arrange
        await ResetStateAsync();
        var user = await CreateTestUser();
        var token = await GetAuthToken(user.Email, "TestPassword123!");
        SetAuthHeaders(token);

        // Act
        var req = new DeleteAccountRequest { Password = "WrongPassword!!!" };
        var response = await Client.PostAsJsonAsync("/api/auth/account/delete", req);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "POST account/delete with valid password anonymizes user.")]
    public async Task DeleteAccount_WithValidPassword_ReturnsOk()
    {
        // Arrange
        await ResetStateAsync();
        var user = await CreateTestUser();
        var token = await GetAuthToken(user.Email, "TestPassword123!");
        SetAuthHeaders(token);

        // Act
        var req = new DeleteAccountRequest { Password = "TestPassword123!" };
        var response = await Client.PostAsJsonAsync("/api/auth/account/delete", req);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        DbContext.ChangeTracker.Clear();
        var updated = await DbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == user.Id);
        updated.ShouldNotBeNull();
        updated!.CanLogin.ShouldBeFalse();
        updated.Email.ShouldContain("account-deleted.invalid");
        updated.Username.ShouldStartWith("deleted_");
    }
}
