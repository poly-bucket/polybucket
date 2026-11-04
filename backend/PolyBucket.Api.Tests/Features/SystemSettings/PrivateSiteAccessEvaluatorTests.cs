using PolyBucket.Api.Middleware;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.SystemSettings;

public class PrivateSiteAccessEvaluatorTests
{
    [Fact(DisplayName = "When public browsing is allowed, any path is permitted.")]
    public void ShouldAllowRequest_PublicSite_ReturnsTrue()
    {
        // Act
        var allowed = PrivateSiteAccessEvaluator.ShouldAllowRequest("/api/models", false, true);

        // Assert
        allowed.ShouldBeTrue();
    }

    [Fact(DisplayName = "When the site is private and the user is authenticated, gated paths are permitted.")]
    public void ShouldAllowRequest_Authenticated_ReturnsTrue()
    {
        // Act
        var allowed = PrivateSiteAccessEvaluator.ShouldAllowRequest("/api/models", true, false);

        // Assert
        allowed.ShouldBeTrue();
    }

    [Fact(DisplayName = "When the site is private and anonymous, /files is blocked.")]
    public void ShouldAllowRequest_AnonymousFiles_ReturnsFalse()
    {
        // Act
        var allowed = PrivateSiteAccessEvaluator.ShouldAllowRequest("/files/sample.stl", false, false);

        // Assert
        allowed.ShouldBeFalse();
    }

    [Theory(DisplayName = "When the site is private and anonymous, allowlisted API paths still pass.")]
    [InlineData("/api/auth/site-access")]
    [InlineData("/api/auth/login")]
    [InlineData("/api/SystemSetup/status")]
    public void ShouldAllowRequest_AnonymousAllowlisted_ReturnsTrue(string path)
    {
        // Act
        var allowed = PrivateSiteAccessEvaluator.ShouldAllowRequest(path, false, false);

        // Assert
        allowed.ShouldBeTrue();
    }
}
