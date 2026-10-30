using System.Net;
using System.Net.Http.Json;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Comments.RemoveCommentDislike;

[Collection("TestCollection")]
public class RemoveCommentDislikeIntegrationTests : BaseIntegrationTest
{
    public RemoveCommentDislikeIntegrationTests(TestCollectionFixture testFixture) : base(testFixture)
    {
    }

    [Fact(DisplayName = "When the request is unauthenticated, RemoveCommentDislike returns Unauthorized.")]
    public async Task RemoveCommentDislike_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        await ResetStateAsync();
        var client = Factory.CreateClient();

        // Act
        var response = await client.DeleteAsync($"/api/comments/{System.Guid.NewGuid()}/dislike");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
