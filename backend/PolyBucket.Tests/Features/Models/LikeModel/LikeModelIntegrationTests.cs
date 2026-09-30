using System;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Models.LikeModel;

[Collection("TestCollection")]
public class LikeModelIntegrationTests : BaseIntegrationTest
{
    public LikeModelIntegrationTests(TestCollectionFixture testFixture) : base(testFixture)
    {
    }

    [Fact(DisplayName = "When liking a model with a valid request, the like model endpoint returns Ok and persists the like.")]
    public async Task LikeModel_WithValidRequest_ReturnsOkAndPersistsLike()
    {
        await ResetStateAsync();
        var client = Factory.CreateClient();
        var owner = await UserFactory.CreateTestUser("owner-like@test.com");
        var liker = await UserFactory.CreateTestUser("liker-like@test.com");
        var model = await ModelFactory.CreateTestModel(owner.Id);

        var token = await GetAuthTokenWithClient(client, liker.Email, "TestPassword123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.PostAsync($"/api/models/{model.Id}/like", null);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        doc.RootElement.GetProperty("likes").GetInt32().ShouldBe(1);

        var updatedModel = await DbContext.Models.AsNoTracking().FirstAsync(m => m.Id == model.Id);
        updatedModel.Likes.ShouldBe(1);

        var like = await DbContext.Likes.AsNoTracking()
            .FirstOrDefaultAsync(l => l.ModelId == model.Id && l.UserId == liker.Id && l.DeletedAt == null);
        like.ShouldNotBeNull();
    }

    [Fact(DisplayName = "When liking a model without authentication, the like model endpoint returns Unauthorized.")]
    public async Task LikeModel_WithoutAuthentication_ReturnsUnauthorized()
    {
        await ResetStateAsync();
        var client = Factory.CreateClient();

        var response = await client.PostAsync($"/api/models/{Guid.NewGuid()}/like", null);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName = "When unliking a liked model, the unlike model endpoint returns Ok and decrements the counter.")]
    public async Task UnlikeModel_WithExistingLike_ReturnsOk()
    {
        await ResetStateAsync();
        var client = Factory.CreateClient();
        var owner = await UserFactory.CreateTestUser("owner-unlike@test.com");
        var liker = await UserFactory.CreateTestUser("liker-unlike@test.com");
        var model = await ModelFactory.CreateTestModel(owner.Id);

        var token = await GetAuthTokenWithClient(client, liker.Email, "TestPassword123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        await client.PostAsync($"/api/models/{model.Id}/like", null);

        var unlikeResponse = await client.DeleteAsync($"/api/models/{model.Id}/like");

        unlikeResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var updatedModel = await DbContext.Models.AsNoTracking().FirstAsync(m => m.Id == model.Id);
        updatedModel.Likes.ShouldBe(0);
    }

    [Fact(DisplayName = "When liking a model that does not exist, the like model endpoint returns NotFound.")]
    public async Task LikeModel_WithNonExistentModel_ReturnsNotFound()
    {
        await ResetStateAsync();
        var client = Factory.CreateClient();
        var user = await UserFactory.CreateTestUser();

        var token = await GetAuthTokenWithClient(client, user.Email, "TestPassword123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.PostAsync($"/api/models/{Guid.NewGuid()}/like", null);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}
