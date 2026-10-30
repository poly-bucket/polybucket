using System.Net;
using System.Net.Http.Json;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Comments.UpdateComment;

[Collection("TestCollection")]
public class UpdateCommentIntegrationTests : BaseIntegrationTest
{
    public UpdateCommentIntegrationTests(TestCollectionFixture testFixture) : base(testFixture)
    {
    }

    [Fact(DisplayName = "When the request is unauthenticated, UpdateComment returns Unauthorized.")]
    public async Task UpdateComment_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        await ResetStateAsync();
        var client = Factory.CreateClient();

        // Act
        var response = await client.PutAsJsonAsync($"/api/comments/{System.Guid.NewGuid()}", new { content = "hi" });

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
