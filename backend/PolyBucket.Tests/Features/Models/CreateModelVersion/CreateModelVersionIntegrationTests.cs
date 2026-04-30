using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using PolyBucket.Api.Features.Models.CreateModelVersion.Domain;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Models.CreateModelVersion;

[Collection("TestCollection")]
public class CreateModelVersionIntegrationTests : BaseIntegrationTest
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public CreateModelVersionIntegrationTests(TestCollectionFixture testFixture) : base(testFixture)
    {
    }

    [Fact(DisplayName = "When creating a model version with a valid request, the create model version endpoint returns Created.")]
    public async Task CreateModelVersion_WithValidRequest_ReturnsCreated()
    {
        await ResetStateAsync();
        var client = Factory.CreateClient();
        var user = await UserFactory.CreateTestUser();
        var model = await ModelFactory.CreateTestModel(user.Id);

        var token = await GetAuthTokenWithClient(client, user.Email, "TestPassword123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var content = new MultipartFormDataContent();
        content.Add(new StringContent("2.0"), "name");
        content.Add(new StringContent("Updated version"), "notes");

        var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes("Test file content"));
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        content.Add(fileContent, "files", "test-v2.stl");

        var response = await client.PostAsync($"/api/models/{model.Id}/versions", content);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);

        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<CreateModelVersionResponse>(responseContent, JsonOptions);

        result.ShouldNotBeNull();
        result!.ModelVersion.ShouldNotBeNull();
        result.ModelVersion.Name.ShouldBe("2.0");
        result.ModelVersion.Notes.ShouldBe("Updated version");
        result.ModelVersion.ModelId.ShouldBe(model.Id);
    }

    [Fact(DisplayName = "When creating a model version without authentication, the create model version endpoint returns Unauthorized.")]
    public async Task CreateModelVersion_WithoutAuthentication_ReturnsUnauthorized()
    {
        await ResetStateAsync();
        var client = Factory.CreateClient();
        var content = new MultipartFormDataContent();
        content.Add(new StringContent("2.0"), "name");

        var response = await client.PostAsync($"/api/models/{Guid.NewGuid()}/versions", content);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName = "When creating a model version for a model that does not exist, the create model version endpoint returns NotFound.")]
    public async Task CreateModelVersion_WithNonExistentModel_ReturnsNotFound()
    {
        await ResetStateAsync();
        var client = Factory.CreateClient();
        var user = await UserFactory.CreateTestUser();

        var token = await GetAuthTokenWithClient(client, user.Email, "TestPassword123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var content = new MultipartFormDataContent();
        content.Add(new StringContent("2.0"), "name");

        var response = await client.PostAsync($"/api/models/{Guid.NewGuid()}/versions", content);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "When creating a model version as a user that does not own the model, the create model version endpoint returns Forbidden.")]
    public async Task CreateModelVersion_WithUnauthorizedUser_ReturnsForbidden()
    {
        await ResetStateAsync();
        var client = Factory.CreateClient();
        var owner = await UserFactory.CreateTestUser("owner@test.com");
        var unauthorizedUser = await UserFactory.CreateTestUser("unauthorized@test.com");
        var model = await ModelFactory.CreateTestModel(owner.Id);

        var token = await GetAuthTokenWithClient(client, unauthorizedUser.Email, "TestPassword123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var content = new MultipartFormDataContent();
        content.Add(new StringContent("2.0"), "name");

        var response = await client.PostAsync($"/api/models/{model.Id}/versions", content);

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact(DisplayName = "When creating a model version without any files, the create model version endpoint returns BadRequest.")]
    public async Task CreateModelVersion_WithoutFiles_ReturnsBadRequest()
    {
        await ResetStateAsync();
        var client = Factory.CreateClient();
        var user = await UserFactory.CreateTestUser();
        var model = await ModelFactory.CreateTestModel(user.Id);

        var token = await GetAuthTokenWithClient(client, user.Email, "TestPassword123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var content = new MultipartFormDataContent();
        content.Add(new StringContent("2.0"), "name");
        content.Add(new StringContent("Updated version"), "notes");

        var response = await client.PostAsync($"/api/models/{model.Id}/versions", content);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "When creating a model version with an invalid file type, the create model version endpoint returns BadRequest.")]
    public async Task CreateModelVersion_WithInvalidFileType_ReturnsBadRequest()
    {
        await ResetStateAsync();
        var client = Factory.CreateClient();
        var user = await UserFactory.CreateTestUser();
        var model = await ModelFactory.CreateTestModel(user.Id);

        var token = await GetAuthTokenWithClient(client, user.Email, "TestPassword123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var content = new MultipartFormDataContent();
        content.Add(new StringContent("2.0"), "name");
        content.Add(new StringContent("Updated version"), "notes");

        var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes("Test file content"));
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
        content.Add(fileContent, "files", "test.txt");

        var response = await client.PostAsync($"/api/models/{model.Id}/versions", content);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }
}
