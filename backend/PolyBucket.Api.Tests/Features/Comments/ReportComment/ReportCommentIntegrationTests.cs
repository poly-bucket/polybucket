using System.Net;
using System.Net.Http.Json;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Comments.ReportComment;

[Collection("TestCollection")]
public class ReportCommentIntegrationTests : BaseIntegrationTest
{
    public ReportCommentIntegrationTests(TestCollectionFixture testFixture) : base(testFixture)
    {
    }

    [Fact(DisplayName = "When the request is unauthenticated, ReportComment returns Unauthorized.")]
    public async Task ReportComment_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        await ResetStateAsync();
        var client = Factory.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync($"/api/comments/{System.Guid.NewGuid()}/report", new { reason = "spam" });

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
