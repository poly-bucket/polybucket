using System.Net;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.SystemSettings.ValidateExtensibleThemeConfiguration;

[Collection("TestCollection")]
public class ValidateExtensibleThemeConfigurationIntegrationTests : BaseIntegrationTest
{
    public ValidateExtensibleThemeConfigurationIntegrationTests(TestCollectionFixture testFixture) : base(testFixture)
    {
    }

    [Fact(DisplayName = "When unauthenticated, ValidateExtensibleThemeConfiguration returns the expected status.")]
    public async Task ValidateExtensibleThemeConfiguration_Unauthenticated_ReturnsExpectedStatus()
    {
        // Arrange
        await ResetStateAsync();
        var client = Factory.CreateClient();
        var request = new HttpRequestMessage(new HttpMethod("POST"), "/api/system-settings/extensible-theme/validate/default");

        // Act
        var response = await client.SendAsync(request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
