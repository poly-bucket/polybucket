using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Features.SystemSettings.Domain;
using PolyBucket.Database.Entities.SystemSettings;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Authentication;

[Collection("TestCollection")]
public class PublicRegistrationIntegrationTests : BaseIntegrationTest
{
    public PublicRegistrationIntegrationTests(TestCollectionFixture testFixture) : base(testFixture)
    {
    }

    private async Task SetPublicRegistrationAsync(bool allowed)
    {
        if (!await DbContext.SystemSetups.AnyAsync())
        {
            DbContext.SystemSetups.Add(new SystemSetup
            {
                Id = Guid.NewGuid(),
                AllowUserRegistration = allowed,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }
        else
        {
            var setup = await DbContext.SystemSetups.FirstAsync();
            setup.AllowUserRegistration = allowed;
        }

        await DbContext.SaveChangesAsync();
    }

    private static object RegisterPayload(string email) => new
    {
        email,
        username = email.Split('@')[0],
        password = "Password123!",
        confirmPassword = "Password123!"
    };

    [Fact(DisplayName = "When public registration is disabled, POST register returns Forbidden and creates no user.")]
    public async Task Register_WhenClosed_Returns403AndNoUser()
    {
        // Arrange
        await ResetStateAsync();
        await SetPublicRegistrationAsync(allowed: false);

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/register", RegisterPayload("closed@test.com"));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await DbContext.Users.CountAsync(u => u.Email == "closed@test.com")).ShouldBe(0);
    }

    [Fact(DisplayName = "When registration is open and verification is required, register succeeds without access tokens in the body.")]
    public async Task Register_WhenVerificationRequired_ReturnsOkWithoutTokens()
    {
        // Arrange
        await ResetStateAsync();
        await SetPublicRegistrationAsync(allowed: true);
        DbContext.SystemSettings.AddRange(
            new SystemSetting { Key = EmailSystemSettingKeys.EmailTransport, Value = nameof(EmailTransportKind.Log) },
            new SystemSetting { Key = EmailSystemSettingKeys.EmailFromAddress, Value = "noreply@polybucket.test" },
            new SystemSetting { Key = EmailSystemSettingKeys.EmailPublicBaseUrl, Value = "https://models.polybucket.test" },
            new SystemSetting { Key = EmailSystemSettingKeys.EmailRequireVerification, Value = "true" });
        await DbContext.SaveChangesAsync();

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/register", RegisterPayload("verify@test.com"));
        var body = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        body.ShouldNotContain("accessToken", Case.Insensitive);
        (await DbContext.Users.CountAsync(u => u.Email == "verify@test.com")).ShouldBe(1);
        using var doc = JsonDocument.Parse(body);
        doc.RootElement.GetProperty("requiresEmailVerification").GetBoolean().ShouldBeTrue();
    }

    [Fact(DisplayName = "When registration is open and verification is not required, register returns access tokens.")]
    public async Task Register_WhenVerificationNotRequired_ReturnsTokens()
    {
        // Arrange
        await ResetStateAsync();
        await SetPublicRegistrationAsync(allowed: true);

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/register", RegisterPayload("open@test.com"));
        var body = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        body.ShouldContain("accessToken", Case.Insensitive);
    }
}
