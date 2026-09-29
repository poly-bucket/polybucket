using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using PolyBucket.Api.Features.Users.GetUserLikedModels.Domain;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Users.GetUserLikedModels;

[Collection("TestCollection")]
public class GetUserLikedModelsIntegrationTests : BaseIntegrationTest
{
    public GetUserLikedModelsIntegrationTests(TestCollectionFixture testFixture) : base(testFixture)
    {
    }

    [Fact(DisplayName = "When getting liked models for a user who liked a model, the liked models endpoint returns the model.")]
    public async Task GetUserLikedModels_AfterLike_ReturnsModel()
    {
        await ResetStateAsync();
        var client = Factory.CreateClient();
        var owner = await UserFactory.CreateTestUser("owner-liked-list@test.com");
        var liker = await UserFactory.CreateTestUser("liker-liked-list@test.com");
        var model = await ModelFactory.CreateTestModel(owner.Id);

        var token = await GetAuthTokenWithClient(client, liker.Email, "TestPassword123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        await client.PostAsync($"/api/models/{model.Id}/like", null);

        var response = await client.GetAsync($"/api/users/{liker.Id}/liked-models?page=1&pageSize=10");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<GetUserLikedModelsResult>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        result.ShouldNotBeNull();
        result!.Models.Count().ShouldBe(1);
        result.Models.First().Id.ShouldBe(model.Id);
    }

    [Fact(DisplayName = "When getting liked models by username, the profile liked models endpoint returns the model.")]
    public async Task GetUserLikedModelsByUsername_AfterLike_ReturnsModel()
    {
        await ResetStateAsync();
        var client = Factory.CreateClient();
        var owner = await UserFactory.CreateTestUser("owner-liked-username@test.com");
        var liker = await UserFactory.CreateTestUser("liker-liked-username@test.com");
        var model = await ModelFactory.CreateTestModel(owner.Id);

        var token = await GetAuthTokenWithClient(client, liker.Email, "TestPassword123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        await client.PostAsync($"/api/models/{model.Id}/like", null);

        var response = await client.GetAsync($"/api/users/profile/{liker.Username}/liked-models?page=1&pageSize=10");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<GetUserLikedModelsResult>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        result.ShouldNotBeNull();
        result!.Models.Count().ShouldBe(1);
        result.Models.First().Id.ShouldBe(model.Id);
    }
}
