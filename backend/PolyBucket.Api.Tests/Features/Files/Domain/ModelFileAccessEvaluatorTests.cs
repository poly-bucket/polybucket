using System.Security.Claims;
using Moq;
using PolyBucket.Api.Features.Files.Domain;
using PolyBucket.Api.Features.ACL.Services;
using PolyBucket.Api.Features.SystemSettings.Services;
using PolyBucket.Database.Entities.Models;
using PolyBucket.Database.Entities.Models.Enums;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Files.Domain;

public class ModelFileAccessEvaluatorTests
{
    private readonly Mock<IPermissionService> _permissionService = new();
    private readonly Mock<ISiteModelDownloadPolicy> _downloadPolicy = new();
    private readonly ModelFileAccessEvaluator _evaluator;

    public ModelFileAccessEvaluatorTests()
    {
        _evaluator = new ModelFileAccessEvaluator(_permissionService.Object, _downloadPolicy.Object);
    }

  private static Model PublicModel() => new()
    {
        Id = Guid.NewGuid(),
        Privacy = PrivacySettings.Public,
        AuthorId = Guid.NewGuid()
    };

    [Fact(DisplayName = "When downloads are disabled, CanDownloadModelAsync returns Disabled.")]
    public async Task CanDownload_WhenDownloadsDisabled_ReturnsDisabled()
    {
        // Arrange
        _downloadPolicy.Setup(p => p.AreModelDownloadsEnabledAsync(It.IsAny<CancellationToken>())).ReturnsAsync(false);

        // Act
        var result = await _evaluator.CanDownloadModelAsync(PublicModel(), new ClaimsPrincipal(), CancellationToken.None);

        // Assert
        result.ShouldBe(ModelDownloadAccessResult.Disabled);
    }

    [Fact(DisplayName = "When anonymous downloads are off, unauthenticated CanDownloadModelAsync returns SignInRequired.")]
    public async Task CanDownload_AnonymousDisallowed_ReturnsSignInRequired()
    {
        // Arrange
        _downloadPolicy.Setup(p => p.AreModelDownloadsEnabledAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _downloadPolicy.Setup(p => p.IsAnonymousModelDownloadAllowedAsync(It.IsAny<CancellationToken>())).ReturnsAsync(false);

        // Act
        var result = await _evaluator.CanDownloadModelAsync(
            PublicModel(),
            new ClaimsPrincipal(new ClaimsIdentity()),
            CancellationToken.None);

        // Assert
        result.ShouldBe(ModelDownloadAccessResult.SignInRequired);
    }

    [Theory(DisplayName = "When anonymous downloads are on, public and unlisted models allow anonymous download.")]
    [InlineData(PrivacySettings.Public)]
    [InlineData(PrivacySettings.Unlisted)]
    public async Task CanDownload_AnonymousAllowed_PublicOrUnlisted(PrivacySettings privacy)
    {
        // Arrange
        _downloadPolicy.Setup(p => p.AreModelDownloadsEnabledAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _downloadPolicy.Setup(p => p.IsAnonymousModelDownloadAllowedAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var model = PublicModel();
        model.Privacy = privacy;

        // Act
        var result = await _evaluator.CanDownloadModelAsync(
            model,
            new ClaimsPrincipal(new ClaimsIdentity()),
            CancellationToken.None);

        // Assert
        result.ShouldBe(ModelDownloadAccessResult.Allowed);
    }

    [Fact(DisplayName = "When anonymous downloads are on, private models return Forbidden for anonymous callers.")]
    public async Task CanDownload_Anonymous_PrivateModel_ReturnsForbidden()
    {
        // Arrange
        _downloadPolicy.Setup(p => p.AreModelDownloadsEnabledAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _downloadPolicy.Setup(p => p.IsAnonymousModelDownloadAllowedAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var model = PublicModel();
        model.Privacy = PrivacySettings.Private;

        // Act
        var result = await _evaluator.CanDownloadModelAsync(
            model,
            new ClaimsPrincipal(new ClaimsIdentity()),
            CancellationToken.None);

        // Assert
        result.ShouldBe(ModelDownloadAccessResult.Forbidden);
    }
}