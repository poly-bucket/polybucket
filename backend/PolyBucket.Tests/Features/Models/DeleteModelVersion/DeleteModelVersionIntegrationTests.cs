using System.Net;
using System.Net.Http.Headers;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Models.DeleteModelVersion;

[Collection("TestCollection")]
public class DeleteModelVersionIntegrationTests : BaseIntegrationTest
{
    public DeleteModelVersionIntegrationTests(TestCollectionFixture testFixture) : base(testFixture)
    {
    }

    [Fact(DisplayName = "When deleting a model version with a valid request, the delete model version endpoint returns NoContent.")]
    public async Task DeleteModelVersion_WithValidRequest_ReturnsNoContent()
    {
        await ResetStateAsync();
        var client = Factory.CreateClient();
        var user = await UserFactory.CreateTestUser();
        var model = await ModelFactory.CreateTestModel(user.Id);
        var version = await ModelFactory.CreateTestModelVersion(model.Id);

        var token = await GetAuthTokenWithClient(client, user.Email, "TestPassword123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.DeleteAsync($"/api/models/{model.Id}/versions/{version.Id}");

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact(DisplayName = "When deleting a model version without authentication, the delete model version endpoint returns Unauthorized.")]
    public async Task DeleteModelVersion_WithoutAuthentication_ReturnsUnauthorized()
    {
        await ResetStateAsync();
        var client = Factory.CreateClient();

        var response = await client.DeleteAsync($"/api/models/{Guid.NewGuid()}/versions/{Guid.NewGuid()}");

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName = "When deleting a model version that does not exist, the delete model version endpoint returns NotFound.")]
    public async Task DeleteModelVersion_WithNonExistentVersion_ReturnsNotFound()
    {
        await ResetStateAsync();
        var client = Factory.CreateClient();
        var user = await UserFactory.CreateTestUser();

        var token = await GetAuthTokenWithClient(client, user.Email, "TestPassword123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.DeleteAsync($"/api/models/{Guid.NewGuid()}/versions/{Guid.NewGuid()}");

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "When deleting a model version as a user that does not own the model, the delete model version endpoint returns Forbidden.")]
    public async Task DeleteModelVersion_WithUnauthorizedUser_ReturnsForbidden()
    {
        await ResetStateAsync();
        var client = Factory.CreateClient();
        var owner = await UserFactory.CreateTestUser("owner@test.com");
        var unauthorizedUser = await UserFactory.CreateTestUser("unauthorized@test.com");
        var model = await ModelFactory.CreateTestModel(owner.Id);
        var version = await ModelFactory.CreateTestModelVersion(model.Id);

        var token = await GetAuthTokenWithClient(client, unauthorizedUser.Email, "TestPassword123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.DeleteAsync($"/api/models/{model.Id}/versions/{version.Id}");

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact(DisplayName = "When deleting a model version under the wrong model id, the delete model version endpoint returns NotFound.")]
    public async Task DeleteModelVersion_WithWrongModelId_ReturnsNotFound()
    {
        await ResetStateAsync();
        var client = Factory.CreateClient();
        var user = await UserFactory.CreateTestUser();
        var model1 = await ModelFactory.CreateTestModel(user.Id);
        var model2 = await ModelFactory.CreateTestModel(user.Id);
        var version = await ModelFactory.CreateTestModelVersion(model1.Id);

        var token = await GetAuthTokenWithClient(client, user.Email, "TestPassword123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.DeleteAsync($"/api/models/{model2.Id}/versions/{version.Id}");

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "When deleting a model version that has already been deleted, the delete model version endpoint returns NotFound.")]
    public async Task DeleteModelVersion_WithAlreadyDeletedVersion_ReturnsNotFound()
    {
        await ResetStateAsync();
        var client = Factory.CreateClient();
        var user = await UserFactory.CreateTestUser();
        var model = await ModelFactory.CreateTestModel(user.Id);
        var version = await ModelFactory.CreateTestModelVersion(model.Id);
        version.DeletedAt = DateTime.UtcNow;
        version.DeletedById = user.Id;
        await DbContext.SaveChangesAsync();

        var token = await GetAuthTokenWithClient(client, user.Email, "TestPassword123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.DeleteAsync($"/api/models/{model.Id}/versions/{version.Id}");

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}
