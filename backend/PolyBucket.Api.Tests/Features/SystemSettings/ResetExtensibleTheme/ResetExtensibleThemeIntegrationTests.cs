using System.Net;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.SystemSettings.ResetExtensibleTheme;

[Collection("TestCollection")]
public class ResetExtensibleThemeIntegrationTests : BaseIntegrationTest
{
    public ResetExtensibleThemeIntegrationTests(TestCollectionFixture testFixture) : base(testFixture)
    {
    }

    [Fact(DisplayName = "When unauthenticated, ResetExtensibleTheme returns the expected status.")]
    public async Task ResetExtensibleTheme_Unauthenticated_ReturnsExpectedStatus()
    {
        // Arrange
        await ResetStateAsync();
        var client = Factory.CreateClient();
        var request = new HttpRequestMessage(new HttpMethod("POST"), "/api/system-settings/extensible-theme/reset");

        // Act
        var response = await client.SendAsync(request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
