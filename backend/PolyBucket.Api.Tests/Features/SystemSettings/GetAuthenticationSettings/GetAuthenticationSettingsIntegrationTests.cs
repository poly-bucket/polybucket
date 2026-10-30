using System.Net;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.SystemSettings.GetAuthenticationSettings;

[Collection("TestCollection")]
public class GetAuthenticationSettingsIntegrationTests : BaseIntegrationTest
{
    public GetAuthenticationSettingsIntegrationTests(TestCollectionFixture testFixture) : base(testFixture)
    {
    }

    [Fact(DisplayName = "When unauthenticated, GetAuthenticationSettings returns the expected status.")]
    public async Task GetAuthenticationSettings_Unauthenticated_ReturnsExpectedStatus()
    {
        // Arrange
        await ResetStateAsync();
        var client = Factory.CreateClient();
        var request = new HttpRequestMessage(new HttpMethod("GET"), "/api/system-settings/auth");

        // Act
        var response = await client.SendAsync(request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
