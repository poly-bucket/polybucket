using System.Net;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.SystemSettings.GetModelConfigurationSettings;

[Collection("TestCollection")]
public class GetModelConfigurationSettingsIntegrationTests : BaseIntegrationTest
{
    public GetModelConfigurationSettingsIntegrationTests(TestCollectionFixture testFixture) : base(testFixture)
    {
    }

    [Fact(DisplayName = "When unauthenticated, GetModelConfigurationSettings returns the expected status.")]
    public async Task GetModelConfigurationSettings_Unauthenticated_ReturnsExpectedStatus()
    {
        // Arrange
        await ResetStateAsync();
        var client = Factory.CreateClient();
        var request = new HttpRequestMessage(new HttpMethod("GET"), "/api/system-settings/model-configuration");

        // Act
        var response = await client.SendAsync(request);

        // Assert
        response.StatusCode.ShouldBeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
    }
}
