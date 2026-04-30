using System.Net;
using System.Net.Http.Headers;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using PolyBucket.Api.Features.Models.GetModelByUserId.Domain;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Models.GetModelByUserId;

[Collection("TestCollection")]
public class GetModelByUserIdIntegrationTests : BaseIntegrationTest
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public GetModelByUserIdIntegrationTests(TestCollectionFixture testFixture) : base(testFixture)
    {
    }

    [Fact(DisplayName = "When getting models by user id with a valid request, the get model by user id endpoint returns Ok with the user's models.")]
    public async Task GetModelByUserId_WithValidRequest_ReturnsOk()
    {
        await ResetStateAsync();
        var client = Factory.CreateClient();
        var user = await UserFactory.CreateTestUser();
        var model1 = await ModelFactory.CreateTestModel(user.Id);
        var model2 = await ModelFactory.CreateTestModel(user.Id);

        var token = await GetAuthTokenWithClient(client, user.Email, "TestPassword123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync($"/api/models/user/{user.Id}?page=1&take=10");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<GetModelByUserIdResponse>(responseContent, JsonOptions);

        result.ShouldNotBeNull();
        result!.Models.ShouldNotBeNull();
        result.Models.Count().ShouldBe(2);
        result.TotalCount.ShouldBe(2);
    }

    [Fact(DisplayName = "When getting models by user id without authentication, the get model by user id endpoint returns Unauthorized.")]
    public async Task GetModelByUserId_WithoutAuthentication_ReturnsUnauthorized()
    {
        await ResetStateAsync();
        var client = Factory.CreateClient();

        var response = await client.GetAsync($"/api/models/user/{Guid.NewGuid()}");

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName = "When getting models for another user without permission, the get model by user id endpoint returns BadRequest.")]
    public async Task GetModelByUserId_WithUnauthorizedUser_ReturnsForbidden()
    {
        await ResetStateAsync();
        var client = Factory.CreateClient();
        var owner = await UserFactory.CreateTestUser("owner@test.com");
        var unauthorizedUser = await UserFactory.CreateTestUser("unauthorized@test.com");
        await ModelFactory.CreateTestModel(owner.Id);

        var token = await GetAuthTokenWithClient(client, unauthorizedUser.Email, "TestPassword123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync($"/api/models/user/{owner.Id}");

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "When getting models by user id with pagination parameters, the get model by user id endpoint returns the correctly paginated results.")]
    public async Task GetModelByUserId_WithPagination_ReturnsCorrectResults()
    {
        await ResetStateAsync();
        var client = Factory.CreateClient();
        var user = await UserFactory.CreateTestUser();
        await ModelFactory.CreateTestModel(user.Id);
        await ModelFactory.CreateTestModel(user.Id);
        await ModelFactory.CreateTestModel(user.Id);

        var token = await GetAuthTokenWithClient(client, user.Email, "TestPassword123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync($"/api/models/user/{user.Id}?page=1&take=2");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<GetModelByUserIdResponse>(responseContent, JsonOptions);

        result.ShouldNotBeNull();
        result!.Models.ShouldNotBeNull();
        result.Models.Count().ShouldBe(2);
        result.TotalCount.ShouldBe(3);
    }

    [Fact(DisplayName = "When requesting another user's models without the required permission, the get model by user id endpoint returns BadRequest.")]
    public async Task GetModelByUserId_WithOtherUserIdWithoutPermission_ReturnsBadRequest()
    {
        await ResetStateAsync();
        var client = Factory.CreateClient();
        var user = await UserFactory.CreateTestUser();

        var token = await GetAuthTokenWithClient(client, user.Email, "TestPassword123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync($"/api/models/user/{Guid.NewGuid()}");

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }
}
