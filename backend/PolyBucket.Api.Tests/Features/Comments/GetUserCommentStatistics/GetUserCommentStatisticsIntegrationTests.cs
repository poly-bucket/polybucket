using System.Net;
using System.Net.Http.Json;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Comments.GetUserCommentStatistics;

[Collection("TestCollection")]
public class GetUserCommentStatisticsIntegrationTests : BaseIntegrationTest
{
    public GetUserCommentStatisticsIntegrationTests(TestCollectionFixture testFixture) : base(testFixture)
    {
    }

    [Fact(DisplayName = "When the request is unauthenticated, GetUserCommentStatistics returns Unauthorized.")]
    public async Task GetUserCommentStatistics_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        await ResetStateAsync();
        var client = Factory.CreateClient();

        // Act
        var response = await client.GetAsync($"/api/comments/user/{System.Guid.NewGuid()}/statistics");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
