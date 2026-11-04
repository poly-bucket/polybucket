using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PolyBucket.Api.Features.SystemSettings.Services;
using PolyBucket.Api.Middleware;
using PolyBucket.Database.Entities.SystemSettings;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.SystemSettings;

[Collection("TestCollection")]
public class PrivateSiteAccessIntegrationTests : BaseIntegrationTest
{
    public PrivateSiteAccessIntegrationTests(TestCollectionFixture testFixture) : base(testFixture)
    {
    }

    private async Task SetPublicBrowsingAsync(bool allowed)
    {
        if (!await DbContext.SystemSetups.AnyAsync())
        {
            DbContext.SystemSetups.Add(new SystemSetup
            {
                Id = Guid.NewGuid(),
                AllowPublicBrowsing = allowed,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }
        else
        {
            var setup = await DbContext.SystemSetups.FirstAsync();
            setup.AllowPublicBrowsing = allowed;
        }

        await DbContext.SaveChangesAsync();
        ServiceScope.ServiceProvider.GetRequiredService<ISitePrivacyState>()
            .Invalidate();
    }

    [Fact(DisplayName = "When the site is private, anonymous GET models returns 401 with the site access marker.")]
    public async Task PrivateSite_AnonymousModels_Returns401WithMarker()
    {
        // Arrange
        await ResetStateAsync();
        await SetPublicBrowsingAsync(allowed: false);
        var client = Factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/models");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        response.Headers.GetValues(PrivateSiteAccessMiddleware.MarkerHeader).Single()
            .ShouldBe(PrivateSiteAccessMiddleware.MarkerValue);
    }

    [Fact(DisplayName = "When the site is private, GET site-access reports public browsing is disabled.")]
    public async Task PrivateSite_SiteAccessEndpoint_ReturnsFalse()
    {
        // Arrange
        await ResetStateAsync();
        await SetPublicBrowsingAsync(allowed: false);
        var client = Factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/auth/site-access");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("allowPublicBrowsing").GetBoolean().ShouldBeFalse();
    }

    [Fact(DisplayName = "When the site is private, anonymous GET under /files returns 401 with the site access marker.")]
    public async Task PrivateSite_AnonymousFiles_Returns401WithMarker()
    {
        // Arrange
        await ResetStateAsync();
        await SetPublicBrowsingAsync(allowed: false);
        var filesDir = Path.Combine(Directory.GetCurrentDirectory(), "files");
        Directory.CreateDirectory(filesDir);
        var fixtureName = $"private-site-test-{Guid.NewGuid():N}.txt";
        var fixturePath = Path.Combine(filesDir, fixtureName);
        await File.WriteAllTextAsync(fixturePath, "integration-test");
        var client = Factory.CreateClient();

        try
        {
            // Act
            var response = await client.GetAsync($"/files/{fixtureName}");

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
            response.Headers.GetValues(PrivateSiteAccessMiddleware.MarkerHeader).Single()
                .ShouldBe(PrivateSiteAccessMiddleware.MarkerValue);
        }
        finally
        {
            if (File.Exists(fixturePath))
            {
                File.Delete(fixturePath);
            }
        }
    }

    [Fact(DisplayName = "When the site is private, an authenticated user may GET models.")]
    public async Task PrivateSite_AuthenticatedModels_ReturnsSuccess()
    {
        // Arrange
        await ResetStateAsync();
        await SetPublicBrowsingAsync(allowed: false);
        var user = await CreateTestUser();
        var token = await GetAuthToken(user.Email!, "TestPassword123!");
        token.ShouldNotBeNullOrWhiteSpace();
        var client = Factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await client.GetAsync("/api/models");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "When public browsing is enabled, anonymous GET models succeeds.")]
    public async Task PublicSite_AnonymousModels_ReturnsOk()
    {
        // Arrange
        await ResetStateAsync();
        await SetPublicBrowsingAsync(allowed: true);
        var client = Factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/models");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}
