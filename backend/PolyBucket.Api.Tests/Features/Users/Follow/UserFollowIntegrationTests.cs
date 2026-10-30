using System;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Features.Users.GetFollowerHistory.Domain;
using PolyBucket.Api.Features.Users.GetUserProfile.Domain;
using PolyBucket.Database.Entities.Notifications;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Users.Follow;

[Collection("TestCollection")]
public class UserFollowIntegrationTests : BaseIntegrationTest
{
    public UserFollowIntegrationTests(TestCollectionFixture testFixture) : base(testFixture)
    {
    }

    [Fact(DisplayName = "When a user follows another user, profile follower counts update.")]
    public async Task Follow_UpdatesProfileCounts()
    {
        await ResetStateAsync();
        var client = Factory.CreateClient();
        var target = await UserFactory.CreateTestUser("follow-target@test.com");
        var follower = await UserFactory.CreateTestUser("follow-follower@test.com");

        var token = await GetAuthTokenWithClient(client, follower.Email, "TestPassword123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var followResponse = await client.PostAsync($"/api/users/{target.Id}/follow", null);
        followResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var profileResponse = await client.GetAsync($"/api/users/profile/by-username/{target.Username}");
        profileResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var profileJson = await profileResponse.Content.ReadAsStringAsync();
        var profile = JsonSerializer.Deserialize<GetUserProfileResponse>(profileJson, JsonOptions);
        profile.ShouldNotBeNull();
        profile!.TotalFollowers.ShouldBe(1);
        profile.IsFollowedByCurrentUser.ShouldBeTrue();
    }

    [Fact(DisplayName = "When a user unfollows, the active follower count drops.")]
    public async Task Unfollow_DropsActiveFollowerCount()
    {
        await ResetStateAsync();
        var client = Factory.CreateClient();
        var target = await UserFactory.CreateTestUser("unfollow-target@test.com");
        var follower = await UserFactory.CreateTestUser("unfollow-follower@test.com");

        var token = await GetAuthTokenWithClient(client, follower.Email, "TestPassword123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        await client.PostAsync($"/api/users/{target.Id}/follow", null);
        await client.DeleteAsync($"/api/users/{target.Id}/follow");

        var profileResponse = await client.GetAsync($"/api/users/profile/by-username/{target.Username}");
        var profile = JsonSerializer.Deserialize<GetUserProfileResponse>(
            await profileResponse.Content.ReadAsStringAsync(),
            JsonOptions);
        profile!.TotalFollowers.ShouldBe(0);
        profile.IsFollowedByCurrentUser.ShouldBeFalse();
    }

    [Fact(DisplayName = "When a profile is private, another user cannot follow it.")]
    public async Task Follow_PrivateProfile_ReturnsForbidden()
    {
        await ResetStateAsync();
        var client = Factory.CreateClient();
        var target = await UserFactory.CreateTestUser("private-target@test.com");
        target.IsProfilePublic = false;
        await DbContext.SaveChangesAsync();
        var follower = await UserFactory.CreateTestUser("private-follower@test.com");

        var token = await GetAuthTokenWithClient(client, follower.Email, "TestPassword123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.PostAsync($"/api/users/{target.Id}/follow", null);
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact(DisplayName = "When another user requests follower history, the API returns 403.")]
    public async Task FollowerHistory_OtherUser_ReturnsForbidden()
    {
        await ResetStateAsync();
        var client = Factory.CreateClient();
        var target = await UserFactory.CreateTestUser("history-target@test.com");
        var other = await UserFactory.CreateTestUser("history-other@test.com");

        var token = await GetAuthTokenWithClient(client, other.Email, "TestPassword123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync($"/api/users/{target.Id}/followers/history?days=30");
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact(DisplayName = "When the owner requests follower history, the API returns a time series.")]
    public async Task FollowerHistory_Owner_ReturnsSeries()
    {
        await ResetStateAsync();
        var client = Factory.CreateClient();
        var owner = await UserFactory.CreateTestUser("history-owner@test.com");
        var follower = await UserFactory.CreateTestUser("history-follower@test.com");

        var followerToken = await GetAuthTokenWithClient(client, follower.Email, "TestPassword123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", followerToken);
        await client.PostAsync($"/api/users/{owner.Id}/follow", null);

        var ownerToken = await GetAuthTokenWithClient(client, owner.Email, "TestPassword123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ownerToken);

        var response = await client.GetAsync($"/api/users/{owner.Id}/followers/history?days=7");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var history = JsonSerializer.Deserialize<GetFollowerHistoryResult>(
            await response.Content.ReadAsStringAsync(),
            JsonOptions);
        history.ShouldNotBeNull();
        history!.Points.Count.ShouldBe(7);
        history.Points.Last().FollowerCount.ShouldBe(1);
    }

    [Fact(DisplayName = "When a user follows another user, one UserFollowed notification is created.")]
    public async Task Follow_CreatesSingleNotification()
    {
        await ResetStateAsync();
        var client = Factory.CreateClient();
        var target = await UserFactory.CreateTestUser("notify-target@test.com");
        var follower = await UserFactory.CreateTestUser("notify-follower@test.com");

        var token = await GetAuthTokenWithClient(client, follower.Email, "TestPassword123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        await client.PostAsync($"/api/users/{target.Id}/follow", null);
        await client.PostAsync($"/api/users/{target.Id}/follow", null);

        var count = await DbContext.Notifications.CountAsync(n =>
            n.UserId == target.Id && n.Type == NotificationType.UserFollowed);
        count.ShouldBe(1);
    }

    [Fact(DisplayName = "When a user unfollows and follows again, a second UserFollowed notification is created.")]
    public async Task Refollow_CreatesSecondNotification()
    {
        await ResetStateAsync();
        var client = Factory.CreateClient();
        var target = await UserFactory.CreateTestUser("refollow-target@test.com");
        var follower = await UserFactory.CreateTestUser("refollow-follower@test.com");

        var token = await GetAuthTokenWithClient(client, follower.Email, "TestPassword123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        await client.PostAsync($"/api/users/{target.Id}/follow", null);
        await client.DeleteAsync($"/api/users/{target.Id}/follow");
        await client.PostAsync($"/api/users/{target.Id}/follow", null);

        var count = await DbContext.Notifications.CountAsync(n =>
            n.UserId == target.Id && n.Type == NotificationType.UserFollowed);
        count.ShouldBe(2);
    }

    private static JsonSerializerOptions JsonOptions => new()
    {
        PropertyNameCaseInsensitive = true
    };
}
