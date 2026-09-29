using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Models.AddTagToModel;

[Collection("TestCollection")]
public class AddTagToModelControllerTests : BaseIntegrationTest
{
    public AddTagToModelControllerTests(TestCollectionFixture testFixture) : base(testFixture)
    {
    }

    [Fact(DisplayName = "When adding a tag as the owner, the add tag controller links the tag once.")]
    public async Task AddTagToModel_ByOwner_ShouldLinkTagOnce()
    {
        // Arrange
        await ResetStateAsync();
        var user = await UserFactory.CreateTestUser();
        var model = await ModelFactory.CreateTestModel(user.Id);
        var client = Factory.CreateClient();
        var token = await GetAuthTokenWithClient(client, user.Email, "TestPassword123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var first = await client.PostAsync($"/api/models/{model.Id}/tags", JsonContent("Printable"));
        var second = await client.PostAsync($"/api/models/{model.Id}/tags", JsonContent("printable"));

        // Assert
        first.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        second.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        DbContext.ChangeTracker.Clear();
        var stored = await DbContext.Models.Include(m => m.Tags).SingleAsync(m => m.Id == model.Id);
        stored.Tags.ShouldHaveSingleItem().Name.ShouldBe("Printable");
        (await DbContext.Tags.CountAsync(t => t.Name.ToLower() == "printable")).ShouldBe(1);
    }

    [Fact(DisplayName = "When adding a tag without authentication, the add tag controller returns Unauthorized.")]
    public async Task AddTagToModel_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Arrange
        await ResetStateAsync();
        var client = Factory.CreateClient();

        // Act
        var response = await client.PostAsync($"/api/models/{Guid.NewGuid()}/tags", JsonContent("printable"));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName = "When adding a tag as a non-owner, the add tag controller returns Forbidden.")]
    public async Task AddTagToModel_ByNonOwner_ShouldReturnForbidden()
    {
        // Arrange
        await ResetStateAsync();
        var owner = await UserFactory.CreateTestUser("owner@test.com");
        var other = await UserFactory.CreateTestUser("other@test.com");
        var model = await ModelFactory.CreateTestModel(owner.Id);
        var client = Factory.CreateClient();
        var token = await GetAuthTokenWithClient(client, other.Email, "TestPassword123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await client.PostAsync($"/api/models/{model.Id}/tags", JsonContent("printable"));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact(DisplayName = "When adding a tag to a model that does not exist, the add tag controller returns NotFound.")]
    public async Task AddTagToModel_WithUnknownModel_ShouldReturnNotFound()
    {
        // Arrange
        await ResetStateAsync();
        var user = await UserFactory.CreateTestUser();
        var client = Factory.CreateClient();
        var token = await GetAuthTokenWithClient(client, user.Email, "TestPassword123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await client.PostAsync($"/api/models/{Guid.NewGuid()}/tags", JsonContent("printable"));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "When adding a tag with an empty name, the add tag controller returns BadRequest.")]
    public async Task AddTagToModel_WithEmptyName_ShouldReturnBadRequest()
    {
        // Arrange
        await ResetStateAsync();
        var user = await UserFactory.CreateTestUser();
        var model = await ModelFactory.CreateTestModel(user.Id);
        var client = Factory.CreateClient();
        var token = await GetAuthTokenWithClient(client, user.Email, "TestPassword123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await client.PostAsync($"/api/models/{model.Id}/tags", JsonContent(""));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    private static StringContent JsonContent(string tagName)
    {
        return new StringContent(JsonSerializer.Serialize(tagName), Encoding.UTF8, "application/json");
    }
}
