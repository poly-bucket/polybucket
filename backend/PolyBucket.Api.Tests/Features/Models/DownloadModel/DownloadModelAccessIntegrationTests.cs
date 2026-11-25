using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PolyBucket.Api.Features.SystemSettings.Services;
using PolyBucket.Api.Tests.Factories;
using PolyBucket.Database.Entities.SystemSettings;
using Shouldly;
using System.Net;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Models.DownloadModel;

[Collection("TestCollection")]
public class DownloadModelAccessIntegrationTests : BaseIntegrationTest
{
    public DownloadModelAccessIntegrationTests(TestCollectionFixture testFixture) : base(testFixture)
    {
    }

    [Fact(DisplayName = "When anonymous downloads are disabled, unauthenticated download returns Unauthorized.")]
    public async Task Download_AnonymousDisabled_ReturnsUnauthorized()
    {
        // Arrange
        await ResetStateAsync();
        await SetDownloadPolicyAsync(allowAnonDownloads: false, enableDownloads: true);
        var client = Factory.CreateClient();

        // Act
        var response = await client.GetAsync($"/api/models/{Guid.NewGuid()}/download");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName = "When model downloads are disabled site-wide, unauthenticated download returns Forbidden.")]
    public async Task Download_SiteDownloadsDisabled_ReturnsForbidden()
    {
        // Arrange
        await ResetStateAsync();
        await SetDownloadPolicyAsync(allowAnonDownloads: true, enableDownloads: false);
        var client = Factory.CreateClient();

        // Act
        var response = await client.GetAsync($"/api/models/{Guid.NewGuid()}/download");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    private async Task SetDownloadPolicyAsync(bool allowAnonDownloads, bool enableDownloads)
    {
        var modelSettings = await DbContext.ModelSettings.FirstOrDefaultAsync();
        if (modelSettings == null)
        {
            modelSettings = new ModelSettings
            {
                Id = Guid.NewGuid(),
                AllowAnonDownloads = allowAnonDownloads,
                EnableModelDownloads = enableDownloads,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            DbContext.ModelSettings.Add(modelSettings);
        }
        else
        {
            modelSettings.AllowAnonDownloads = allowAnonDownloads;
            modelSettings.EnableModelDownloads = enableDownloads;
            modelSettings.UpdatedAt = DateTime.UtcNow;
        }

        await DbContext.SaveChangesAsync();

        using var scope = Factory.Services.CreateScope();
        scope.ServiceProvider.GetRequiredService<ISiteModelDownloadPolicy>().Invalidate();
    }
}