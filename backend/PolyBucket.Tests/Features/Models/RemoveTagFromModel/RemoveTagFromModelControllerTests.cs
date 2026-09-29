using System.Net;
using System.Net.Http.Headers;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Features.Models.AddTagToModel.Domain;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Models.RemoveTagFromModel;

[Collection("TestCollection")]
public class RemoveTagFromModelControllerTests : BaseIntegrationTest
{
    public RemoveTagFromModelControllerTests(TestCollectionFixture testFixture) : base(testFixture)
    {
    }

    [Fact(DisplayName = "When removing a tag as the owner, the remove tag controller unlinks the tag and keeps the tag row.")]
    public async Task RemoveTagFromModel_ByOwner_ShouldUnlinkTag()
    {
        // Arrange
        await ResetStateAsync();
        var user = await UserFactory.CreateTestUser();
        var model = await ModelFactory.CreateTestModel(user.Id);
        var tag = new Tag
        {
            Id = Guid.NewGuid(),
            Name = "printable",
            Color = string.Empty,
            CreatedAt = DateTime.UtcNow,
            CreatedById = user.Id,
            UpdatedById = user.Id
        };
        model.Tags.Add(tag);
        await DbContext.SaveChangesAsync();
        var client = Factory.CreateClient();
        var token = await GetAuthTokenWithClient(client, user.Email, "TestPassword123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await client.DeleteAsync($"/api/models/{model.Id}/tags/{tag.Id}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        DbContext.ChangeTracker.Clear();
        var stored = await DbContext.Models.Include(m => m.Tags).SingleAsync(m => m.Id == model.Id);
        stored.Tags.ShouldBeEmpty();
        (await DbContext.Tags.AnyAsync(t => t.Id == tag.Id)).ShouldBeTrue();
    }

    [Fact(DisplayName = "When removing a tag that is not linked, the remove tag controller returns NotFound.")]
    public async Task RemoveTagFromModel_WithMissingLink_ShouldReturnNotFound()
    {
        // Arrange
        await ResetStateAsync();
        var user = await UserFactory.CreateTestUser();
        var model = await ModelFactory.CreateTestModel(user.Id);
        var client = Factory.CreateClient();
        var token = await GetAuthTokenWithClient(client, user.Email, "TestPassword123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await client.DeleteAsync($"/api/models/{model.Id}/tags/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "When removing a tag without authentication, the remove tag controller returns Unauthorized.")]
    public async Task RemoveTagFromModel_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Arrange
        await ResetStateAsync();
        var client = Factory.CreateClient();

        // Act
        var response = await client.DeleteAsync($"/api/models/{Guid.NewGuid()}/tags/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName = "When removing a tag as a non-owner, the remove tag controller returns Forbidden.")]
    public async Task RemoveTagFromModel_ByNonOwner_ShouldReturnForbidden()
    {
        // Arrange
        await ResetStateAsync();
        var owner = await UserFactory.CreateTestUser("owner@test.com");
        var other = await UserFactory.CreateTestUser("other@test.com");
        var model = await ModelFactory.CreateTestModel(owner.Id);
        var tag = new Tag
        {
            Id = Guid.NewGuid(),
            Name = "printable",
            Color = string.Empty,
            CreatedAt = DateTime.UtcNow,
            CreatedById = owner.Id,
            UpdatedById = owner.Id
        };
        model.Tags.Add(tag);
        await DbContext.SaveChangesAsync();
        var client = Factory.CreateClient();
        var token = await GetAuthTokenWithClient(client, other.Email, "TestPassword123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await client.DeleteAsync($"/api/models/{model.Id}/tags/{tag.Id}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact(DisplayName = "When removing a tag from a model that does not exist, the remove tag controller returns NotFound.")]
    public async Task RemoveTagFromModel_WithUnknownModel_ShouldReturnNotFound()
    {
        // Arrange
        await ResetStateAsync();
        var user = await UserFactory.CreateTestUser();
        var client = Factory.CreateClient();
        var token = await GetAuthTokenWithClient(client, user.Email, "TestPassword123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await client.DeleteAsync($"/api/models/{Guid.NewGuid()}/tags/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}
