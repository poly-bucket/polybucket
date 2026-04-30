using System.Net;
using System.Net.Http.Headers;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Models.DeleteModel;

[Collection("TestCollection")]
public class DeleteModelIntegrationTests : BaseIntegrationTest
{
    public DeleteModelIntegrationTests(TestCollectionFixture testFixture) : base(testFixture)
    {
    }

    [Fact(DisplayName = "When deleting a model with a valid request, the delete model endpoint returns NoContent.")]
    public async Task DeleteModel_WithValidRequest_ReturnsNoContent()
    {
        await ResetStateAsync();
        var client = Factory.CreateClient();
        var user = await UserFactory.CreateTestUser();
        var model = await ModelFactory.CreateTestModel(user.Id);

        var token = await GetAuthTokenWithClient(client, user.Email, "TestPassword123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.DeleteAsync($"/api/models/{model.Id}");

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact(DisplayName = "When deleting a model without authentication, the delete model endpoint returns Unauthorized.")]
    public async Task DeleteModel_WithoutAuthentication_ReturnsUnauthorized()
    {
        await ResetStateAsync();
        var client = Factory.CreateClient();

        var response = await client.DeleteAsync($"/api/models/{Guid.NewGuid()}");

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName = "When deleting a model that does not exist, the delete model endpoint returns NotFound.")]
    public async Task DeleteModel_WithNonExistentModel_ReturnsNotFound()
    {
        await ResetStateAsync();
        var client = Factory.CreateClient();
        var user = await UserFactory.CreateTestUser();

        var token = await GetAuthTokenWithClient(client, user.Email, "TestPassword123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.DeleteAsync($"/api/models/{Guid.NewGuid()}");

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "When deleting a model as a user that does not own the model, the delete model endpoint returns Forbidden.")]
    public async Task DeleteModel_WithUnauthorizedUser_ReturnsForbidden()
    {
        await ResetStateAsync();
        var client = Factory.CreateClient();
        var owner = await UserFactory.CreateTestUser("owner@test.com");
        var unauthorizedUser = await UserFactory.CreateTestUser("unauthorized@test.com");
        var model = await ModelFactory.CreateTestModel(owner.Id);

        var token = await GetAuthTokenWithClient(client, unauthorizedUser.Email, "TestPassword123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.DeleteAsync($"/api/models/{model.Id}");

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact(DisplayName = "When deleting a model that has already been deleted, the delete model endpoint returns NotFound.")]
    public async Task DeleteModel_WithAlreadyDeletedModel_ReturnsNotFound()
    {
        await ResetStateAsync();
        var client = Factory.CreateClient();
        var user = await UserFactory.CreateTestUser();
        var model = await ModelFactory.CreateTestModel(user.Id);
        model.DeletedAt = DateTime.UtcNow;
        model.DeletedById = user.Id;
        await DbContext.SaveChangesAsync();

        var token = await GetAuthTokenWithClient(client, user.Email, "TestPassword123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.DeleteAsync($"/api/models/{model.Id}");

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}
