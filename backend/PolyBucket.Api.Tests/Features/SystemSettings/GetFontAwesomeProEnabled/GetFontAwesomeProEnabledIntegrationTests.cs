using System.Net;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.SystemSettings.GetFontAwesomeProEnabled;

[Collection("TestCollection")]
public class GetFontAwesomeProEnabledIntegrationTests : BaseIntegrationTest
{
    public GetFontAwesomeProEnabledIntegrationTests(TestCollectionFixture testFixture) : base(testFixture)
    {
    }

    [Fact(DisplayName = "When unauthenticated, GetFontAwesomeProEnabled returns the expected status.")]
    public async Task GetFontAwesomeProEnabled_Unauthenticated_ReturnsExpectedStatus()
    {
        // Arrange
        await ResetStateAsync();
        var client = Factory.CreateClient();
        var request = new HttpRequestMessage(new HttpMethod("GET"), "/api/admin/fontawesome-settings/pro-enabled");

        // Act
        var response = await client.SendAsync(request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
