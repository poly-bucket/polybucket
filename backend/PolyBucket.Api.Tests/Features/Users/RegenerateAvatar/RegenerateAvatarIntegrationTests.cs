using System;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Features.Users.RegenerateAvatar.Domain;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Users.RegenerateAvatar;

[Collection("TestCollection")]
public class RegenerateAvatarIntegrationTests : BaseIntegrationTest
{
    public RegenerateAvatarIntegrationTests(TestCollectionFixture testFixture) : base(testFixture)
    {
    }

    [Fact(DisplayName = "When the caller submits client minidenticon SVG, the stored avatar matches that markup.")]
    public async Task RegenerateAvatar_ClientSvg_PersistsSubmittedMarkup()
    {
        // Arrange
        await ResetStateAsync();
        var client = Factory.CreateClient();
        var user = await UserFactory.CreateTestUser("avatar-svg@test.com");
        var token = await GetAuthTokenWithClient(client, user.Email, "TestPassword123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        const string clientSvg =
            "<svg viewBox=\"-1.5 -1.5 8 8\" xmlns=\"http://www.w3.org/2000/svg\" fill=\"hsl(80 50% 50%)\">" +
            "<rect x=\"0\" y=\"0\" width=\"1\" height=\"1\"/></svg>";

        var request = new RegenerateAvatarRequest
        {
            Salt = "pattern-seed",
            Avatar = clientSvg
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/users/avatar/regenerate", request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var returnedAvatar = doc.RootElement.GetProperty("avatar").GetString();
        returnedAvatar.ShouldNotBeNullOrWhiteSpace();
        returnedAvatar.ShouldContain("viewBox=\"-1.5 -1.5 8 8\"");

        var updated = await DbContext.Users.AsNoTracking().SingleAsync(u => u.Id == user.Id);
        updated.Avatar.ShouldBe(returnedAvatar);
        updated.Avatar.ShouldContain("viewBox=\"-1.5 -1.5 8 8\"");
    }

    [Fact(DisplayName = "When only a salt is sent, the API uses the server avatar generator.")]
    public async Task RegenerateAvatar_SaltOnly_UsesServerGenerator()
    {
        // Arrange
        await ResetStateAsync();
        var client = Factory.CreateClient();
        var user = await UserFactory.CreateTestUser("avatar-salt@test.com");
        var token = await GetAuthTokenWithClient(client, user.Email, "TestPassword123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var request = new RegenerateAvatarRequest { Salt = "salt-only-seed" };

        // Act
        var response = await client.PostAsJsonAsync("/api/users/avatar/regenerate", request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var updated = await DbContext.Users.AsNoTracking().SingleAsync(u => u.Id == user.Id);
        updated.Avatar.ShouldNotBeNullOrWhiteSpace();
        updated.Avatar.ShouldContain("viewBox=\"0 0 5 5\"");
        updated.Avatar.ShouldNotContain("viewBox=\"-1.5 -1.5 8 8\"");
    }
}