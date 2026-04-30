using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using PolyBucket.Api.Common.Models.Enums;
using PolyBucket.Api.Features.Models.UpdateModel.Domain;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Models.UpdateModel;

[Collection("TestCollection")]
public class UpdateModelIntegrationTests : BaseIntegrationTest
{
    private static readonly JsonSerializerOptions UpdateModelJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public UpdateModelIntegrationTests(TestCollectionFixture testFixture) : base(testFixture)
    {
    }

    [Fact(DisplayName = "When updating a model with a valid request, the update model endpoint returns Ok.")]
    public async Task UpdateModel_WithValidRequest_ReturnsOk()
    {
        await ResetStateAsync();
        var client = Factory.CreateClient();
        var user = await UserFactory.CreateTestUser();
        var model = await ModelFactory.CreateTestModel(user.Id);

        var token = await GetAuthTokenWithClient(client, user.Email, "TestPassword123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var updateRequest = new
        {
            Name = "Updated Model Name",
            Description = "Updated Description",
            Privacy = PrivacySettings.Private,
            License = LicenseTypes.MIT
        };

        var content = new StringContent(
            JsonSerializer.Serialize(updateRequest),
            Encoding.UTF8,
            "application/json");

        var response = await client.PutAsync($"/api/models/{model.Id}", content);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var responseContent = await response.Content.ReadAsStringAsync();
        var envelope = JsonSerializer.Deserialize<UpdateModelResponse>(responseContent, UpdateModelJsonOptions);

        envelope.ShouldNotBeNull();
        envelope!.Model.ShouldNotBeNull();
        envelope.Model.Id.ShouldBe(model.Id);
        envelope.Model.Name.ShouldBe("Updated Model Name");
        envelope.Model.Description.ShouldBe("Updated Description");
        envelope.Model.Privacy.ShouldBe(PrivacySettings.Private);
        envelope.Model.License.ShouldBe(LicenseTypes.MIT);
    }

    [Fact(DisplayName = "When updating a model without authentication, the update model endpoint returns Unauthorized.")]
    public async Task UpdateModel_WithoutAuthentication_ReturnsUnauthorized()
    {
        await ResetStateAsync();
        var client = Factory.CreateClient();
        var updateRequest = new { Name = "Updated Name" };
        var content = new StringContent(
            JsonSerializer.Serialize(updateRequest),
            Encoding.UTF8,
            "application/json");

        var response = await client.PutAsync($"/api/models/{Guid.NewGuid()}", content);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName = "When updating a model that does not exist, the update model endpoint returns NotFound.")]
    public async Task UpdateModel_WithNonExistentModel_ReturnsNotFound()
    {
        await ResetStateAsync();
        var client = Factory.CreateClient();
        var user = await UserFactory.CreateTestUser();

        var token = await GetAuthTokenWithClient(client, user.Email, "TestPassword123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var updateRequest = new { Name = "Updated Name" };
        var content = new StringContent(
            JsonSerializer.Serialize(updateRequest),
            Encoding.UTF8,
            "application/json");

        var response = await client.PutAsync($"/api/models/{Guid.NewGuid()}", content);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "When updating a model as a user that does not own the model, the update model endpoint returns Forbidden.")]
    public async Task UpdateModel_WithUnauthorizedUser_ReturnsForbidden()
    {
        await ResetStateAsync();
        var client = Factory.CreateClient();
        var owner = await UserFactory.CreateTestUser("owner@test.com");
        var unauthorizedUser = await UserFactory.CreateTestUser("unauthorized@test.com");
        var model = await ModelFactory.CreateTestModel(owner.Id);

        var token = await GetAuthTokenWithClient(client, unauthorizedUser.Email, "TestPassword123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var updateRequest = new { Name = "Updated Name" };
        var content = new StringContent(
            JsonSerializer.Serialize(updateRequest),
            Encoding.UTF8,
            "application/json");

        var response = await client.PutAsync($"/api/models/{model.Id}", content);

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact(DisplayName = "When updating a model with an empty name, the update model endpoint returns BadRequest.")]
    public async Task UpdateModel_WithEmptyName_ReturnsBadRequest()
    {
        await ResetStateAsync();
        var client = Factory.CreateClient();
        var user = await UserFactory.CreateTestUser();
        var model = await ModelFactory.CreateTestModel(user.Id);

        var token = await GetAuthTokenWithClient(client, user.Email, "TestPassword123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var updateRequest = new { Name = "" };
        var content = new StringContent(
            JsonSerializer.Serialize(updateRequest),
            Encoding.UTF8,
            "application/json");

        var response = await client.PutAsync($"/api/models/{model.Id}", content);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }
}
