using System.Net;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.SystemSettings.TestFontAwesomeLicense;

[Collection("TestCollection")]
public class TestFontAwesomeLicenseIntegrationTests : BaseIntegrationTest
{
    public TestFontAwesomeLicenseIntegrationTests(TestCollectionFixture testFixture) : base(testFixture)
    {
    }

    [Fact(DisplayName = "When unauthenticated, TestFontAwesomeLicense returns the expected status.")]
    public async Task TestFontAwesomeLicense_Unauthenticated_ReturnsExpectedStatus()
    {
        // Arrange
        await ResetStateAsync();
        var client = Factory.CreateClient();
        var request = new HttpRequestMessage(new HttpMethod("POST"), "/api/admin/fontawesome-settings/test-license");

        // Act
        var response = await client.SendAsync(request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
