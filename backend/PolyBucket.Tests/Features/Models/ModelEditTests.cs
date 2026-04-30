using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Common.Models.Enums;
using PolyBucket.Api.Features.Models.UpdateModel.Domain;
using Xunit;

namespace PolyBucket.Tests.Features.Models;

[Collection("TestCollection")]
public class ModelEditTests : BaseIntegrationTest
{
    private static readonly JsonSerializerOptions UpdateModelJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public ModelEditTests(TestCollectionFixture testFixture) : base(testFixture)
    {
    }

    [Fact(DisplayName = "When updating a model with valid data, the update model controller updates the model.")]
    public async Task UpdateModel_WithValidData_ShouldUpdateModel()
    {
        await ResetStateAsync();
        var user = await UserFactory.CreateTestUser();
        var model = await ModelFactory.CreateTestModel("Original Name", "Original Description");
        model.AuthorId = user.Id;
        await DbContext.SaveChangesAsync();

        var client = Factory.CreateClient();
        var token = await GetAuthTokenWithClient(client, user.Email, "TestPassword123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var updateRequest = new
        {
            name = "Updated Name",
            description = "Updated Description",
            license = (int)LicenseTypes.GPLv3,
            privacy = (int)PrivacySettings.Private,
            aiGenerated = true,
            wip = true,
            nsfw = false,
            isRemix = false
        };

        var content = new StringContent(JsonSerializer.Serialize(updateRequest), Encoding.UTF8, "application/json");

        var response = await client.PutAsync($"/api/models/{model.Id}", content);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var responseContent = await response.Content.ReadAsStringAsync();
        var envelope = JsonSerializer.Deserialize<UpdateModelResponse>(responseContent, UpdateModelJsonOptions);
        var updatedModel = envelope?.Model;

        Assert.NotNull(updatedModel);
        Assert.Equal("Updated Name", updatedModel!.Name);
        Assert.Equal("Updated Description", updatedModel.Description);
        Assert.Equal(LicenseTypes.GPLv3, updatedModel.License);
        Assert.Equal(PrivacySettings.Private, updatedModel.Privacy);
        Assert.True(updatedModel.AIGenerated);
        Assert.True(updatedModel.WIP);
        Assert.False(updatedModel.NSFW);
    }

    [Fact(DisplayName = "When updating a model without authentication, the update model controller returns Unauthorized.")]
    public async Task UpdateModel_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        await ResetStateAsync();
        var user = await UserFactory.CreateTestUser();
        var model = await ModelFactory.CreateTestModel();
        model.AuthorId = user.Id;
        await DbContext.SaveChangesAsync();

        var client = Factory.CreateClient();
        var updateRequest = new { name = "Updated Name" };
        var content = new StringContent(JsonSerializer.Serialize(updateRequest), Encoding.UTF8, "application/json");

        var response = await client.PutAsync($"/api/models/{model.Id}", content);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact(DisplayName = "When updating a model as a non-owner, the update model controller returns Forbidden.")]
    public async Task UpdateModel_ByNonOwner_ShouldReturnForbidden()
    {
        await ResetStateAsync();
        var owner = await UserFactory.CreateTestUser("owner@test.com");
        var nonOwner = await UserFactory.CreateTestUser("nonowner@test.com");
        var model = await ModelFactory.CreateTestModel();
        model.AuthorId = owner.Id;
        await DbContext.SaveChangesAsync();

        var client = Factory.CreateClient();
        var token = await GetAuthTokenWithClient(client, nonOwner.Email, "TestPassword123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var updateRequest = new { name = "Updated Name" };
        var content = new StringContent(JsonSerializer.Serialize(updateRequest), Encoding.UTF8, "application/json");

        var response = await client.PutAsync($"/api/models/{model.Id}", content);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact(DisplayName = "When updating a model that does not exist, the update model controller returns NotFound.")]
    public async Task UpdateModel_NonExistentModel_ShouldReturnNotFound()
    {
        await ResetStateAsync();
        var user = await UserFactory.CreateTestUser();
        var client = Factory.CreateClient();
        var token = await GetAuthTokenWithClient(client, user.Email, "TestPassword123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var updateRequest = new { name = "Updated Name" };
        var content = new StringContent(JsonSerializer.Serialize(updateRequest), Encoding.UTF8, "application/json");
        var nonExistentId = Guid.NewGuid();

        var response = await client.PutAsync($"/api/models/{nonExistentId}", content);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact(DisplayName = "When updating a model that has been deleted, the update model controller returns BadRequest.")]
    public async Task UpdateModel_DeletedModel_ShouldReturnBadRequest()
    {
        await ResetStateAsync();
        var user = await UserFactory.CreateTestUser();
        var model = await ModelFactory.CreateTestModel();
        model.AuthorId = user.Id;
        model.DeletedAt = DateTime.UtcNow;
        await DbContext.SaveChangesAsync();

        var client = Factory.CreateClient();
        var token = await GetAuthTokenWithClient(client, user.Email, "TestPassword123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var updateRequest = new { name = "Updated Name" };
        var content = new StringContent(JsonSerializer.Serialize(updateRequest), Encoding.UTF8, "application/json");

        var response = await client.PutAsync($"/api/models/{model.Id}", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact(DisplayName = "When updating a model with a partial payload, the update model controller updates only the specified fields.")]
    public async Task UpdateModel_PartialUpdate_ShouldUpdateOnlySpecifiedFields()
    {
        await ResetStateAsync();
        var user = await UserFactory.CreateTestUser();
        var model = await ModelFactory.CreateTestModel("Original Name", "Original Description");
        model.AuthorId = user.Id;
        model.License = LicenseTypes.MIT;
        model.Privacy = PrivacySettings.Public;
        await DbContext.SaveChangesAsync();

        var client = Factory.CreateClient();
        var token = await GetAuthTokenWithClient(client, user.Email, "TestPassword123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var updateRequest = new { name = "Updated Name" };
        var content = new StringContent(JsonSerializer.Serialize(updateRequest), Encoding.UTF8, "application/json");

        var response = await client.PutAsync($"/api/models/{model.Id}", content);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var responseContent = await response.Content.ReadAsStringAsync();
        var envelope = JsonSerializer.Deserialize<UpdateModelResponse>(responseContent, UpdateModelJsonOptions);
        var updatedModel = envelope?.Model;

        Assert.NotNull(updatedModel);
        Assert.Equal("Updated Name", updatedModel!.Name);
        Assert.Equal("Original Description", updatedModel.Description);
        Assert.Equal(LicenseTypes.MIT, updatedModel.License);
        Assert.Equal(PrivacySettings.Public, updatedModel.Privacy);
    }

    [Fact(DisplayName = "When updating a model with remix data, the update model controller updates the remix-related fields.")]
    public async Task UpdateModel_WithRemixData_ShouldUpdateRemixFields()
    {
        await ResetStateAsync();
        var user = await UserFactory.CreateTestUser();
        var model = await ModelFactory.CreateTestModel();
        model.AuthorId = user.Id;
        await DbContext.SaveChangesAsync();

        var client = Factory.CreateClient();
        var token = await GetAuthTokenWithClient(client, user.Email, "TestPassword123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var updateRequest = new
        {
            isRemix = true,
            remixUrl = "https://example.com/original-model"
        };
        var content = new StringContent(JsonSerializer.Serialize(updateRequest), Encoding.UTF8, "application/json");

        var response = await client.PutAsync($"/api/models/{model.Id}", content);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var responseContent = await response.Content.ReadAsStringAsync();
        var envelope = JsonSerializer.Deserialize<UpdateModelResponse>(responseContent, UpdateModelJsonOptions);
        var updatedModel = envelope?.Model;

        Assert.NotNull(updatedModel);
        Assert.True(updatedModel!.IsRemix);
        Assert.Equal("https://example.com/original-model", updatedModel.RemixUrl);
    }
}
