using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Moq;
using PolyBucket.Api.Features.Federation.Services;
using PolyBucket.Api.Features.SystemSettings.Services;
using PolyBucket.Email;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Federation.Services;

public class FederationInstanceIdentityServiceTests
{
    [Fact(DisplayName = "Public base URL from email settings is preferred over Instance:BaseUrl.")]
    public async Task GetSnapshot_PrefersEmailPublicBaseUrl()
    {
        // Arrange
        var setupId = Guid.NewGuid();
        var policy = new Mock<ISiteFederationPolicy>();
        policy.Setup(p => p.GetSnapshotAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SiteFederationPolicySnapshot(
                setupId,
                true,
                "Node A",
                "Desc",
                "admin@test.com"));

        var email = new Mock<IEmailSettingsResolver>();
        email.Setup(e => e.GetEffectiveSettingsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EffectiveEmailSettings
            {
                PublicBaseUrl = "https://models.example.com"
            });

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Instance:BaseUrl"] = "https://ignored.example.com"
            })
            .Build();

        var httpContextAccessor = new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext()
        };

        var service = new FederationInstanceIdentityService(
            policy.Object,
            email.Object,
            configuration,
            httpContextAccessor);

        // Act
        var snapshot = await service.GetSnapshotAsync();

        // Assert
        snapshot.InstanceId.ShouldBe(setupId);
        snapshot.InstanceName.ShouldBe("Node A");
        snapshot.BaseUrl.ShouldBe("https://models.example.com");
        snapshot.IsReadyForPeering.ShouldBeTrue();
    }

    [Fact(DisplayName = "When federation is enabled but instance name is missing, peering is not ready.")]
    public async Task GetSnapshot_MissingName_NotReadyForPeering()
    {
        // Arrange
        var policy = new Mock<ISiteFederationPolicy>();
        policy.Setup(p => p.GetSnapshotAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SiteFederationPolicySnapshot(
                Guid.NewGuid(),
                true,
                "   ",
                "",
                ""));

        var email = new Mock<IEmailSettingsResolver>();
        email.Setup(e => e.GetEffectiveSettingsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EffectiveEmailSettings { PublicBaseUrl = "https://models.example.com" });

        var service = new FederationInstanceIdentityService(
            policy.Object,
            email.Object,
            new ConfigurationBuilder().Build(),
            new HttpContextAccessor { HttpContext = new DefaultHttpContext() });

        // Act
        var snapshot = await service.GetSnapshotAsync();

        // Assert
        snapshot.IsReadyForPeering.ShouldBeFalse();
    }
}