using Microsoft.AspNetCore.Http;
using PolyBucket.Api.Features.Models.UploadValidation.Domain;
using PolyBucket.Api.Features.SystemSettings.Services;
using PolyBucket.Database.Entities.Models.Enums;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Models.Common;

public class ModelMetadataValidatorTests
{
    private readonly ModelMetadataValidator _validator = new();

  private static ModelMetadataPolicySnapshot PermissivePolicy() =>
        new(
            RequireModelDescription: false,
            MinDescriptionLength: 0,
            MaxDescriptionLength: 2000,
            RequireLicenseSelection: false,
            AllowCustomLicenses: true,
            AllowNsfwContent: true,
            AllowAiGeneratedContent: true,
            RequireThumbnail: false,
            MaxTagsPerModel: 10,
            RequireModelTags: false,
            MaxCategoriesPerModel: 3,
            RequireCategorySelection: false);

    [Fact(DisplayName = "When NSFW is disallowed, create metadata validation returns an error for NSFW models.")]
    public void ValidateCreate_NsfwDenied_ReturnsError()
    {
        // Arrange
        var policy = PermissivePolicy() with { AllowNsfwContent = false };

        // Act
        var error = _validator.ValidateCreateMetadata(
            policy,
            description: "ok",
            license: "mit",
            parsedLicense: LicenseTypes.MIT,
            nsfw: true,
            aiGenerated: false,
            files: [CreateFormFile("model.stl")]);

        // Assert
        error.ShouldNotBeNull();
        error.ShouldContain("NSFW");
    }

    [Fact(DisplayName = "When thumbnail is required, create metadata validation requires an image in the bundle.")]
    public void ValidateCreate_RequireThumbnail_RequiresImage()
    {
        // Arrange
        var policy = PermissivePolicy() with { RequireThumbnail = true };

        // Act
        var withoutImage = _validator.ValidateCreateMetadata(
            policy,
            description: null,
            license: null,
            parsedLicense: null,
            nsfw: false,
            aiGenerated: false,
            files: [CreateFormFile("model.stl")]);
        var withImage = _validator.ValidateCreateMetadata(
            policy,
            description: null,
            license: null,
            parsedLicense: null,
            nsfw: false,
            aiGenerated: false,
            files: [CreateFormFile("model.stl"), CreateFormFile("thumb.png")]);

        // Assert
        withoutImage.ShouldNotBeNull();
        withImage.ShouldBeNull();
    }

    [Fact(DisplayName = "When require tags is on, removing the last tag is rejected.")]
    public void ValidateRemoveTag_LastTag_ReturnsError()
    {
        // Arrange
        var policy = PermissivePolicy() with { RequireModelTags = true };

        // Act
        var error = _validator.ValidateRemoveTag(policy, currentTagCount: 1);

        // Assert
        error.ShouldNotBeNull();
    }

    [Fact(DisplayName = "When at max tags, adding another tag is rejected.")]
    public void ValidateAddTag_AtMax_ReturnsError()
    {
        // Arrange
        var policy = PermissivePolicy() with { MaxTagsPerModel = 2 };

        // Act
        var error = _validator.ValidateAddTag(policy, currentTagCount: 2, isNewLink: true);

        // Assert
        error.ShouldNotBeNull();
    }

    private static IFormFile CreateFormFile(string fileName)
    {
        var content = new MemoryStream([0x01, 0x02]);
        return new FormFile(content, 0, content.Length, "Files", fileName);
    }
}