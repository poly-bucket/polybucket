using System.Net;
using System.Net.Http.Json;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Comments.ModerateAllUserComments;

[Collection("TestCollection")]
public class ModerateAllUserCommentsIntegrationTests : BaseIntegrationTest
{
    public ModerateAllUserCommentsIntegrationTests(TestCollectionFixture testFixture) : base(testFixture)
    {
    }

    [Fact(DisplayName = "When the request is unauthenticated, ModerateAllUserComments returns Unauthorized.")]
    public async Task ModerateAllUserComments_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        await ResetStateAsync();
        var client = Factory.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync($"/api/comments/user/{System.Guid.NewGuid()}/moderate-all", new { reason = "spam" });

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
