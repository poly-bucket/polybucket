using PolyBucket.Marketplace.Api.Services;
using Shouldly;
using Xunit;

namespace PolyBucket.Marketplace.Tests.Services;

public class PluginRepositoryManifestServiceTests
{
    [Fact(DisplayName = "When repository URL is not https GitHub, archive URL build fails.")]
    public void TryBuildGitArchiveUrl_InvalidUrl_ReturnsFalse()
    {
        // Arrange
        var repositoryUrl = "ftp://example.com/repo";

        // Act
        var ok = PluginRepositoryManifestService.TryBuildGitArchiveUrl(repositoryUrl, "main", out _, out var error);

        // Assert
        ok.ShouldBeFalse();
        error.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact(DisplayName = "When repository URL is valid GitHub https, archive URL is built.")]
    public void TryBuildGitArchiveUrl_GitHubUrl_ReturnsArchiveUrl()
    {
        // Arrange
        var repositoryUrl = "https://github.com/poly-bucket/polybucket";

        // Act
        var ok = PluginRepositoryManifestService.TryBuildGitArchiveUrl(repositoryUrl, "main", out var archiveUrl, out _);

        // Assert
        ok.ShouldBeTrue();
        archiveUrl.ShouldBe("https://github.com/poly-bucket/polybucket/archive/refs/heads/main.zip");
    }
}
