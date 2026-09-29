using PolyBucket.Api.Features.ModelModeration.Domain;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.ModelModeration;

public class ModelModerationPolicyTests
{
    private readonly ModelModerationPolicy _policy = new();

    [Fact]
    public void IsModerationRequired_WhenAllFlagsEnabled_ReturnsTrue()
    {
        // Arrange
        var settings = new ModelModerationSettingsSnapshot
        {
            RequireModeration = true,
            RequireUploadModeration = true,
            RequireModeratorApproval = true
        };

        // Act
        var result = _policy.IsModerationRequired(settings);

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void IsModerationRequired_WhenSiteModerationDisabled_ReturnsFalse()
    {
        // Arrange
        var settings = new ModelModerationSettingsSnapshot
        {
            RequireModeration = false,
            RequireUploadModeration = true,
            RequireModeratorApproval = true
        };

        // Act
        var result = _policy.IsModerationRequired(settings);

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void ShouldAutoApprove_WhenAutoApproveModels_ReturnsTrue()
    {
        // Arrange
        var settings = new ModelModerationSettingsSnapshot
        {
            RequireModeration = true,
            RequireUploadModeration = true,
            RequireModeratorApproval = true,
            AutoApproveModels = true
        };

        // Act
        var result = _policy.ShouldAutoApprove(settings, authorEmailVerified: false);

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void ShouldAutoApprove_WhenVerifiedUserAndFlagEnabled_ReturnsTrue()
    {
        // Arrange
        var settings = new ModelModerationSettingsSnapshot
        {
            RequireModeration = true,
            RequireUploadModeration = true,
            RequireModeratorApproval = true,
            AutoApproveVerifiedUsers = true
        };

        // Act
        var result = _policy.ShouldAutoApprove(settings, authorEmailVerified: true);

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void ShouldAutoApprove_WhenModerationRequiredAndNoBypass_ReturnsFalse()
    {
        // Arrange
        var settings = new ModelModerationSettingsSnapshot
        {
            RequireModeration = true,
            RequireUploadModeration = true,
            RequireModeratorApproval = true
        };

        // Act
        var result = _policy.ShouldAutoApprove(settings, authorEmailVerified: false);

        // Assert
        result.ShouldBeFalse();
    }
}
