using System;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Models.ModelReactions;

[Collection("TestCollection")]
public class ModelReactionsIntegrationTests : BaseIntegrationTest
{
    public ModelReactionsIntegrationTests(TestCollectionFixture testFixture) : base(testFixture)
    {
    }

    [Fact(DisplayName = "When a user likes then dislikes a model, counters follow each step.")]
    public async Task LikeThenDislike_UpdatesCounters()
    {
        await ResetStateAsync();
        var client = Factory.CreateClient();
        var owner = await UserFactory.CreateTestUser("owner-react@test.com");
        var voter = await UserFactory.CreateTestUser("voter-react@test.com");
        var model = await ModelFactory.CreateTestModel(owner.Id);

        var token = await GetAuthTokenWithClient(client, voter.Email, "TestPassword123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var like = await client.PostAsync($"/api/models/{model.Id}/like", null);
        like.StatusCode.ShouldBe(HttpStatusCode.OK);
        using (var likeDoc = JsonDocument.Parse(await like.Content.ReadAsStringAsync()))
        {
            likeDoc.RootElement.GetProperty("likes").GetInt32().ShouldBe(1);
            likeDoc.RootElement.GetProperty("userHasLiked").GetBoolean().ShouldBeTrue();
        }

        var dislike = await client.PostAsync($"/api/models/{model.Id}/dislike", null);
        dislike.StatusCode.ShouldBe(HttpStatusCode.OK);
        using (var dislikeDoc = JsonDocument.Parse(await dislike.Content.ReadAsStringAsync()))
        {
            dislikeDoc.RootElement.GetProperty("likes").GetInt32().ShouldBe(0);
            dislikeDoc.RootElement.GetProperty("dislikes").GetInt32().ShouldBe(1);
            dislikeDoc.RootElement.GetProperty("userHasDisliked").GetBoolean().ShouldBeTrue();
        }

        var updated = await DbContext.Models.AsNoTracking().SingleAsync(m => m.Id == model.Id);
        updated.Likes.ShouldBe(0);
        updated.Dislikes.ShouldBe(1);
    }

    [Fact(DisplayName = "When the model author tries to like their model, the API returns Forbidden.")]
    public async Task SelfLike_ReturnsForbidden()
    {
        await ResetStateAsync();
        var client = Factory.CreateClient();
        var owner = await UserFactory.CreateTestUser("owner-self-like@test.com");
        var model = await ModelFactory.CreateTestModel(owner.Id);

        var token = await GetAuthTokenWithClient(client, owner.Email, "TestPassword123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.PostAsync($"/api/models/{model.Id}/like", null);

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }
}
