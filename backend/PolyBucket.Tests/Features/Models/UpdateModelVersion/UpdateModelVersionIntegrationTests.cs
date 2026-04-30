using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using PolyBucket.Api.Features.Models.UpdateModelVersion.Domain;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Models.UpdateModelVersion;

[Collection("TestCollection")]
public class UpdateModelVersionIntegrationTests : BaseIntegrationTest
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public UpdateModelVersionIntegrationTests(TestCollectionFixture testFixture) : base(testFixture)
    {
    }

    [Fact(DisplayName = "When updating a model version with a valid request, the update model version endpoint returns Ok.")]
    public async Task UpdateModelVersion_WithValidRequest_ReturnsOk()
    {
        await ResetStateAsync();
        var client = Factory.CreateClient();
        var user = await UserFactory.CreateTestUser();
        var model = await ModelFactory.CreateTestModel(user.Id);
        var version = await ModelFactory.CreateTestModelVersion(model.Id);

        var token = await GetAuthTokenWithClient(client, user.Email, "TestPassword123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var updateRequest = new
        {
            Name = "2.1",
            Notes = "Updated version notes"
        };

        var content = new StringContent(
            JsonSerializer.Serialize(updateRequest),
            Encoding.UTF8,
            "application/json");

        var response = await client.PutAsync($"/api/models/{model.Id}/versions/{version.Id}", content);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var responseContent = await response.Content.ReadAsStringAsync();
        var envelope = JsonSerializer.Deserialize<UpdateModelVersionResponse>(responseContent, JsonOptions);

        envelope.ShouldNotBeNull();
        envelope!.ModelVersion.ShouldNotBeNull();
        envelope.ModelVersion.Id.ShouldBe(version.Id);
        envelope.ModelVersion.Name.ShouldBe("2.1");
        envelope.ModelVersion.Notes.ShouldBe("Updated version notes");
        envelope.ModelVersion.ModelId.ShouldBe(model.Id);
    }

    [Fact(DisplayName = "When updating a model version without authentication, the update model version endpoint returns Unauthorized.")]
    public async Task UpdateModelVersion_WithoutAuthentication_ReturnsUnauthorized()
    {
        await ResetStateAsync();
        var client = Factory.CreateClient();
        var updateRequest = new { Name = "2.1" };
        var content = new StringContent(
            JsonSerializer.Serialize(updateRequest),
            Encoding.UTF8,
            "application/json");

        var response = await client.PutAsync($"/api/models/{Guid.NewGuid()}/versions/{Guid.NewGuid()}", content);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName = "When updating a model version that does not exist, the update model version endpoint returns NotFound.")]
    public async Task UpdateModelVersion_WithNonExistentVersion_ReturnsNotFound()
    {
        await ResetStateAsync();
        var client = Factory.CreateClient();
        var user = await UserFactory.CreateTestUser();

        var token = await GetAuthTokenWithClient(client, user.Email, "TestPassword123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var updateRequest = new { Name = "2.1" };
        var content = new StringContent(
            JsonSerializer.Serialize(updateRequest),
            Encoding.UTF8,
            "application/json");

        var response = await client.PutAsync($"/api/models/{Guid.NewGuid()}/versions/{Guid.NewGuid()}", content);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "When updating a model version as a user that does not own the model, the update model version endpoint returns Forbidden.")]
    public async Task UpdateModelVersion_WithUnauthorizedUser_ReturnsForbidden()
    {
        await ResetStateAsync();
        var client = Factory.CreateClient();
        var owner = await UserFactory.CreateTestUser("owner@test.com");
        var unauthorizedUser = await UserFactory.CreateTestUser("unauthorized@test.com");
        var model = await ModelFactory.CreateTestModel(owner.Id);
        var version = await ModelFactory.CreateTestModelVersion(model.Id);

        var token = await GetAuthTokenWithClient(client, unauthorizedUser.Email, "TestPassword123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var updateRequest = new { Name = "2.1" };
        var content = new StringContent(
            JsonSerializer.Serialize(updateRequest),
            Encoding.UTF8,
            "application/json");

        var response = await client.PutAsync($"/api/models/{model.Id}/versions/{version.Id}", content);

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact(DisplayName = "When updating a model version under the wrong model id, the update model version endpoint returns NotFound.")]
    public async Task UpdateModelVersion_WithWrongModelId_ReturnsNotFound()
    {
        await ResetStateAsync();
        var client = Factory.CreateClient();
        var user = await UserFactory.CreateTestUser();
        var model1 = await ModelFactory.CreateTestModel(user.Id);
        var model2 = await ModelFactory.CreateTestModel(user.Id);
        var version = await ModelFactory.CreateTestModelVersion(model1.Id);

        var token = await GetAuthTokenWithClient(client, user.Email, "TestPassword123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var updateRequest = new { Name = "2.1" };
        var content = new StringContent(
            JsonSerializer.Serialize(updateRequest),
            Encoding.UTF8,
            "application/json");

        var response = await client.PutAsync($"/api/models/{model2.Id}/versions/{version.Id}", content);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}
