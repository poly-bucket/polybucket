using System.Net;
using System.Net.Http.Json;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Comments.ModerateComment;

[Collection("TestCollection")]
public class ModerateCommentIntegrationTests : BaseIntegrationTest
{
    public ModerateCommentIntegrationTests(TestCollectionFixture testFixture) : base(testFixture)
    {
    }

    [Fact(DisplayName = "When the request is unauthenticated, ModerateComment returns Unauthorized.")]
    public async Task ModerateComment_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        await ResetStateAsync();
        var client = Factory.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync($"/api/comments/{System.Guid.NewGuid()}/moderate", new { reason = "spam" });

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
