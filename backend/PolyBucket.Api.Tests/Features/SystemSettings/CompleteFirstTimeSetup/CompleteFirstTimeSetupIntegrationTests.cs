using System.Net;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.SystemSettings.CompleteFirstTimeSetup;

[Collection("TestCollection")]
public class CompleteFirstTimeSetupIntegrationTests : BaseIntegrationTest
{
    public CompleteFirstTimeSetupIntegrationTests(TestCollectionFixture testFixture) : base(testFixture)
    {
    }

    [Fact(DisplayName = "When unauthenticated, CompleteFirstTimeSetup returns the expected status.")]
    public async Task CompleteFirstTimeSetup_Unauthenticated_ReturnsExpectedStatus()
    {
        // Arrange
        await ResetStateAsync();
        var client = Factory.CreateClient();
        var request = new HttpRequestMessage(new HttpMethod("POST"), "/api/SystemSetup/complete");

        // Act
        var response = await client.SendAsync(request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
