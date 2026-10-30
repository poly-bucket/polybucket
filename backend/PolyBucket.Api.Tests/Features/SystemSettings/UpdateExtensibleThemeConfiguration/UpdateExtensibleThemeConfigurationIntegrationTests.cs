using System.Net;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.SystemSettings.UpdateExtensibleThemeConfiguration;

[Collection("TestCollection")]
public class UpdateExtensibleThemeConfigurationIntegrationTests : BaseIntegrationTest
{
    public UpdateExtensibleThemeConfigurationIntegrationTests(TestCollectionFixture testFixture) : base(testFixture)
    {
    }

    [Fact(DisplayName = "When unauthenticated, UpdateExtensibleThemeConfiguration returns the expected status.")]
    public async Task UpdateExtensibleThemeConfiguration_Unauthenticated_ReturnsExpectedStatus()
    {
        // Arrange
        await ResetStateAsync();
        var client = Factory.CreateClient();
        var request = new HttpRequestMessage(new HttpMethod("PUT"), "/api/system-settings/extensible-theme/configuration");

        // Act
        var response = await client.SendAsync(request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
