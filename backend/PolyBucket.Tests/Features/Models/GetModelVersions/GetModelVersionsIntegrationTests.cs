using System;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using PolyBucket.Api.Common.Models.Enums;
using PolyBucket.Api.Features.ModelModeration.Domain;
using PolyBucket.Api.Features.Models.CreateModelVersion.Domain;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Models.GetModelVersions;

[Collection("TestCollection")]
public class GetModelVersionsIntegrationTests : BaseIntegrationTest
{
    public GetModelVersionsIntegrationTests(TestCollectionFixture testFixture) : base(testFixture)
    {
    }

    private void AddVersion(Guid modelId, int number, bool deleted = false)
    {
        DbContext.ModelVersions.Add(new ModelVersion
        {
            Id = Guid.NewGuid(),
            ModelId = modelId,
            Name = $"v{number}",
            Notes = $"Notes {number}",
            VersionNumber = number,
            CreatedAt = DateTime.UtcNow.AddMinutes(number),
            DeletedAt = deleted ? DateTime.UtcNow : null
        });
    }

    [Fact(DisplayName = "When a public model has versions, anyone can list them newest first without deleted versions.")]
    public async Task PublicModel_ListsVersionsNewestFirst()
    {
        // Arrange
        await ResetStateAsync();
        var author = await CreateTestUser();
        var model = await ModelFactory.CreateTestModel("Versioned", "desc", author.Id);
        model.IsPublic = true;
        AddVersion(model.Id, 1);
        AddVersion(model.Id, 2);
        AddVersion(model.Id, 3, deleted: true);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await Client.GetAsync($"/api/models/{model.Id}/versions");
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var names = body.RootElement.GetProperty("versions").EnumerateArray().Select(v => v.GetProperty("name").GetString()).ToList();
        names.ShouldBe(new[] { "v2", "v1" });
    }

    [Fact(DisplayName = "When a model is pending moderation, other users get 404 while the owner can list its versions.")]
    public async Task PendingModel_HiddenFromOthers_VisibleToOwner()
    {
        // Arrange
        await ResetStateAsync();
        var author = await CreateTestUser("owner-versions@polybucket.test", "Password123!");
        var other = await CreateTestUser("other-versions@polybucket.test", "Password123!");
        var model = await ModelFactory.CreateTestModel("Pending versions", "desc", author.Id);
        model.IsPublic = true;
        AddVersion(model.Id, 1);
        DbContext.ModelModerationRecords.Add(new ModelModerationRecord { Id = Guid.NewGuid(), ModelId = model.Id, Status = ModelModerationStatus.Pending });
        await DbContext.SaveChangesAsync();
        var ownerToken = await GetAuthToken(author.Email, "Password123!");
        var otherToken = await GetAuthToken(other.Email, "Password123!");

        // Act
        var anonymous = await Client.GetAsync($"/api/models/{model.Id}/versions");
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", otherToken);
        var asOther = await Client.GetAsync($"/api/models/{model.Id}/versions");
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ownerToken);
        var asOwner = await Client.GetAsync($"/api/models/{model.Id}/versions");

        // Assert
        anonymous.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        asOther.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        asOwner.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "When a model is private, other signed-in users get 404.")]
    public async Task PrivateModel_OtherUser_NotFound()
    {
        // Arrange
        await ResetStateAsync();
        var author = await CreateTestUser();
        var other = await CreateTestUser("private-viewer@polybucket.test", "Password123!");
        var model = await ModelFactory.CreateTestModel("Private versions", "desc", author.Id);
        model.Privacy = PrivacySettings.Private;
        AddVersion(model.Id, 1);
        await DbContext.SaveChangesAsync();
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await GetAuthToken(other.Email, "Password123!"));

        // Act
        var response = await Client.GetAsync($"/api/models/{model.Id}/versions");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}
