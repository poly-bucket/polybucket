using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PolyBucket.Api.Features.SystemSettings.Services;
using PolyBucket.Api.Tests.Factories;
using PolyBucket.Database.Entities.Models;
using PolyBucket.Database.Entities.SystemSettings;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Models;

[Collection("TestCollection")]
public class ModelVersioningAccessIntegrationTests : BaseIntegrationTest
{
    public ModelVersioningAccessIntegrationTests(TestCollectionFixture testFixture) : base(testFixture)
    {
    }

    [Fact(DisplayName = "When site versioning is disabled, creating a version returns Forbidden.")]
    public async Task CreateVersion_VersioningDisabled_ReturnsForbidden()
    {
        // Arrange
        await ResetStateAsync();
        await SetVersioningEnabledAsync(false);
        var client = Factory.CreateClient();
        var user = await UserFactory.CreateTestUser();
        var model = await ModelFactory.CreateTestModel(user.Id);
        var token = await GetAuthTokenWithClient(client, user.Email, "TestPassword123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var content = new MultipartFormDataContent();
        content.Add(new StringContent("2.0"), "name");
        var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes("solid test\nendsolid test\n"));
        content.Add(fileContent, "files", "v2.stl");

        // Act
        var response = await client.PostAsync($"/api/models/{model.Id}/versions", content);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact(DisplayName = "When site versioning is disabled, strangers listing versions get an empty list.")]
    public async Task ListVersions_VersioningDisabled_StrangerGetsEmpty()
    {
        // Arrange
        await ResetStateAsync();
        await SetVersioningEnabledAsync(false);
        var owner = await UserFactory.CreateTestUser();
        var model = await ModelFactory.CreateTestModel(owner.Id);
        model.IsPublic = true;
        await DbContext.SaveChangesAsync();
        await SeedModelVersionAsync(model.Id);

        var client = Factory.CreateClient();
        var stranger = await UserFactory.CreateTestUser();
        var token = await GetAuthTokenWithClient(client, stranger.Email, "TestPassword123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await client.GetAsync($"/api/models/{model.Id}/versions");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        json.ShouldContain("\"versions\": []");
    }

    [Fact(DisplayName = "When site versioning is disabled, the owner still lists stored versions.")]
    public async Task ListVersions_VersioningDisabled_OwnerSeesHistory()
    {
        // Arrange
        await ResetStateAsync();
        await SetVersioningEnabledAsync(false);
        var owner = await UserFactory.CreateTestUser();
        var model = await ModelFactory.CreateTestModel(owner.Id);
        await SeedModelVersionAsync(model.Id, name: "v2");

        var client = Factory.CreateClient();
        var token = await GetAuthTokenWithClient(client, owner.Email, "TestPassword123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await client.GetAsync($"/api/models/{model.Id}/versions");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        json.ShouldContain("v2");
    }

    private async Task SetVersioningEnabledAsync(bool enabled)
    {
        var modelSettings = await DbContext.ModelSettings.FirstOrDefaultAsync();
        if (modelSettings == null)
        {
            modelSettings = new ModelSettings
            {
                Id = Guid.NewGuid(),
                EnableModelVersioning = enabled,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            DbContext.ModelSettings.Add(modelSettings);
        }
        else
        {
            modelSettings.EnableModelVersioning = enabled;
            modelSettings.UpdatedAt = DateTime.UtcNow;
        }

        await DbContext.SaveChangesAsync();

        using var scope = Factory.Services.CreateScope();
        scope.ServiceProvider.GetRequiredService<ISiteModelVersioningPolicy>().Invalidate();
    }

    private async Task SeedModelVersionAsync(Guid modelId, string name = "v1")
    {
        DbContext.ModelVersions.Add(new ModelVersion
        {
            Id = Guid.NewGuid(),
            ModelId = modelId,
            Name = name,
            VersionNumber = 1,
            CreatedAt = DateTime.UtcNow
        });
        await DbContext.SaveChangesAsync();
    }
}