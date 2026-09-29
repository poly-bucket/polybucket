using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Common.Models.Enums;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.ModelModeration.Domain;
using PolyBucket.Api.Features.SystemSettings.Domain;
using Shouldly;
using System;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace PolyBucket.Tests.Features.ModelModeration;

[Collection("TestCollection")]
public class ModelModerationIntegrationTests : BaseIntegrationTest
{
    public ModelModerationIntegrationTests(TestCollectionFixture testFixture) : base(testFixture)
    {
    }

    [Fact(DisplayName = "When moderation is required, a new public model is queued until approved.")]
    public async Task CreateModel_WithModerationEnabled_AppearsInQueueThenPublicAfterApprove()
    {
        // Arrange
        await ResetStateAsync();
        await SeedModerationSettingsAsync(requireModeration: true, autoApprove: false);

        var author = await UserFactory.CreateTestUser("author@test.com");
        await DbContext.SaveChangesAsync();

        var authorClient = Factory.CreateClient();
        var authorToken = await GetAuthTokenWithClient(authorClient, author.Email, "TestPassword123!");
        authorClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authorToken);

        var content = new MultipartFormDataContent();
        content.Add(new StringContent("Queued Model"), "Name");
        content.Add(new StringContent("Needs approval"), "Description");
        content.Add(new StringContent("Public"), "Privacy");
        content.Add(new StringContent("CCBy4"), "License");
        var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes("solid test endsolid"));
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        content.Add(fileContent, "Files", "test.stl");

        // Act
        var createResponse = await authorClient.PostAsync("/api/models", content);

        // Assert
        createResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
        var createJson = await createResponse.Content.ReadAsStringAsync();
        var modelId = ExtractModelId(createJson);

        var publicList = await authorClient.GetAsync("/api/models?page=1&take=20");
        publicList.StatusCode.ShouldBe(HttpStatusCode.OK);
        var publicListBody = await publicList.Content.ReadAsStringAsync();
        publicListBody.ShouldNotContain(modelId.ToString());

        var moderator = await CreateModeratorUserAsync();
        var modClient = Factory.CreateClient();
        var modToken = await GetAuthTokenWithClient(modClient, moderator.Email, "TestPassword123!");
        modClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", modToken);

        var queueResponse = await modClient.GetAsync("/api/moderation/models?page=1&pageSize=20");
        queueResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var queueJson = await queueResponse.Content.ReadAsStringAsync();
        queueJson.ShouldContain("Queued Model");

        var approveResponse = await modClient.PostAsync($"/api/moderation/models/{modelId}/approve", null);
        approveResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var publicAfterApprove = await authorClient.GetAsync("/api/models?page=1&take=20");
        var publicBody = await publicAfterApprove.Content.ReadAsStringAsync();
        publicBody.ShouldContain("Queued Model");
    }

    [Fact(DisplayName = "When user lacks queue permission, moderation models endpoint returns Forbidden.")]
    public async Task GetQueue_WithoutPermission_ReturnsForbidden()
    {
        // Arrange
        await ResetStateAsync();
        var user = await UserFactory.CreateTestUser("regular@test.com");
        await DbContext.SaveChangesAsync();

        var client = Factory.CreateClient();
        var token = await GetAuthTokenWithClient(client, user.Email, "TestPassword123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await client.GetAsync("/api/moderation/models?page=1&pageSize=20");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    private async Task SeedModerationSettingsAsync(bool requireModeration, bool autoApprove)
    {
        if (!await DbContext.SystemSetups.AnyAsync())
        {
            DbContext.SystemSetups.Add(new SystemSetup
            {
                Id = Guid.NewGuid(),
                RequireModeration = requireModeration,
                AutoApproveModels = autoApprove,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }
        else
        {
            var setup = await DbContext.SystemSetups.FirstAsync();
            setup.RequireModeration = requireModeration;
            setup.AutoApproveModels = autoApprove;
        }

        if (!await DbContext.ModelSettings.AnyAsync())
        {
            DbContext.ModelSettings.Add(new ModelSettings
            {
                Id = Guid.NewGuid(),
                RequireUploadModeration = true,
                RequireModeratorApproval = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        await DbContext.SaveChangesAsync();
    }

    private async Task<User> CreateModeratorUserAsync()
    {
        var moderatorRole = await DbContext.Roles.FirstAsync(r => r.Name == "Moderator");
        var user = await UserFactory.CreateTestUser("moderator@test.com");
        user.RoleId = moderatorRole.Id;
        await DbContext.SaveChangesAsync();

        var moderationPermissions = PermissionConstants.DefaultRoles.MODERATOR.Values
            .SelectMany(p => p)
            .Distinct()
            .ToList();

        var permissionEntities = await DbContext.Permissions
            .Where(p => moderationPermissions.Contains(p.Name))
            .ToListAsync();

        foreach (var permission in permissionEntities)
        {
            if (!await DbContext.RolePermissions.AnyAsync(rp => rp.RoleId == moderatorRole.Id && rp.PermissionId == permission.Id))
            {
                DbContext.RolePermissions.Add(new RolePermission
                {
                    RoleId = moderatorRole.Id,
                    PermissionId = permission.Id,
                    IsGranted = true
                });
            }
        }

        await DbContext.SaveChangesAsync();
        return user;
    }

    private static Guid ExtractModelId(string json)
    {
        using var doc = System.Text.Json.JsonDocument.Parse(json);
        foreach (var prop in doc.RootElement.EnumerateObject())
        {
            if (prop.NameEquals("model") && prop.Value.TryGetProperty("id", out var idProp))
            {
                return Guid.Parse(idProp.GetString()!);
            }
        }

        throw new InvalidOperationException("Model id not found in response");
    }
}
