using System.Net;
using System.Net.Http.Json;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Comments.CreateComment;

[Collection("TestCollection")]
public class CreateCommentIntegrationTests : BaseIntegrationTest
{
    public CreateCommentIntegrationTests(TestCollectionFixture testFixture) : base(testFixture)
    {
    }

    [Fact(DisplayName = "When the request is unauthenticated, CreateComment returns Unauthorized.")]
    public async Task CreateComment_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        await ResetStateAsync();
        var client = Factory.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync("/api/comments", new { target = new { targetId = System.Guid.NewGuid(), targetType = "Model" }, content = "hi" });

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
