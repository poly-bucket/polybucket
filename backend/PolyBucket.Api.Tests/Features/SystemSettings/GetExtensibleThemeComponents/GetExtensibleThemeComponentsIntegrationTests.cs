using System.Net;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.SystemSettings.GetExtensibleThemeComponents;

[Collection("TestCollection")]
public class GetExtensibleThemeComponentsIntegrationTests : BaseIntegrationTest
{
    public GetExtensibleThemeComponentsIntegrationTests(TestCollectionFixture testFixture) : base(testFixture)
    {
    }

    [Fact(DisplayName = "When unauthenticated, GetExtensibleThemeComponents returns the expected status.")]
    public async Task GetExtensibleThemeComponents_Unauthenticated_ReturnsExpectedStatus()
    {
        // Arrange
        await ResetStateAsync();
        var client = Factory.CreateClient();
        var request = new HttpRequestMessage(new HttpMethod("GET"), "/api/system-settings/extensible-theme/components/default");

        // Act
        var response = await client.SendAsync(request);

        // Assert
        response.StatusCode.ShouldBeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
    }
}
