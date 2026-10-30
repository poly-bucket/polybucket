using System.Net;
using System.Net.Http.Json;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Comments.DeleteComment;

[Collection("TestCollection")]
public class DeleteCommentIntegrationTests : BaseIntegrationTest
{
    public DeleteCommentIntegrationTests(TestCollectionFixture testFixture) : base(testFixture)
    {
    }

    [Fact(DisplayName = "When the request is unauthenticated, DeleteComment returns Unauthorized.")]
    public async Task DeleteComment_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        await ResetStateAsync();
        var client = Factory.CreateClient();

        // Act
        var response = await client.DeleteAsync($"/api/comments/{System.Guid.NewGuid()}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
