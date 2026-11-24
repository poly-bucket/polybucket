using System;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Features.Users.GetUserProfile.Domain;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Users.GetUserProfile;

[Collection("TestCollection")]
public class GetUserProfilePrivacyIntegrationTests : BaseIntegrationTest
{
    public GetUserProfilePrivacyIntegrationTests(TestCollectionFixture testFixture) : base(testFixture)
    {
    }

    [Fact(DisplayName = "When a profile is private, an anonymous caller receives a private profile stub.")]
    public async Task GetProfile_Private_Anonymous_ReturnsStub()
    {
        await ResetStateAsync();
        var target = await UserFactory.CreateTestUser("private-view@test.com");
        target.IsProfilePublic = false;
        await DbContext.SaveChangesAsync();

        var response = await Factory.CreateClient().GetAsync($"/api/users/profile/by-username/{target.Username}");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        doc.RootElement.TryGetProperty("message", out _).ShouldBeTrue();
        doc.RootElement.TryGetProperty("createdAt", out _).ShouldBeFalse();
    }

    [Fact(DisplayName = "When a profile is private, the owner receives a full profile.")]
    public async Task GetProfile_Private_Owner_ReturnsFullProfile()
    {
        await ResetStateAsync();
        var target = await UserFactory.CreateTestUser("private-owner@test.com");
        target.IsProfilePublic = false;
        await DbContext.SaveChangesAsync();

        var client = Factory.CreateClient();
        var token = await GetAuthTokenWithClient(client, target.Email, "TestPassword123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync($"/api/users/profile/by-username/{target.Username}");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var profile = JsonSerializer.Deserialize<GetUserProfileResponse>(
            await response.Content.ReadAsStringAsync(),
            JsonOptions);
        profile.ShouldNotBeNull();
        profile!.CreatedAt.ShouldNotBe(default);
        profile.IsProfilePublic.ShouldBeFalse();
        profile.Email.ShouldBe(target.Email);
    }

    [Fact(DisplayName = "When show statistics is disabled, strangers do not receive counts.")]
    public async Task GetProfile_ShowStatisticsOff_Stranger_StatsHidden()
    {
        await ResetStateAsync();
        var target = await UserFactory.CreateTestUser("stats-hidden@test.com");
        target.ShowStatistics = false;
        await DbContext.SaveChangesAsync();

        var response = await Factory.CreateClient().GetAsync($"/api/users/profile/by-username/{target.Username}");
        var profile = JsonSerializer.Deserialize<GetUserProfileResponse>(
            await response.Content.ReadAsStringAsync(),
            JsonOptions);
        profile!.TotalModels.ShouldBeNull();
        profile.TotalFollowers.ShouldBeNull();
    }

    private static JsonSerializerOptions JsonOptions => new()
    {
        PropertyNameCaseInsensitive = true
    };
}