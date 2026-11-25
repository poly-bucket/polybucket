using Microsoft.AspNetCore.Http;
using PolyBucket.Api.Features.Models.Common;
using PolyBucket.Api.Features.SystemSettings.Services;
using PolyBucket.Database.Entities.Models.Enums;

namespace PolyBucket.Api.Features.Models.UploadValidation.Domain;

public sealed class ModelMetadataValidator : IModelMetadataValidator
{
    private static readonly string[] ImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp" };

    public string? ValidateCreateMetadata(
        ModelMetadataPolicySnapshot policy,
        string? description,
        string? license,
        LicenseTypes? parsedLicense,
        bool nsfw,
        bool aiGenerated,
        bool remix,
        string? remixUrl,
        IFormFile[] files)
    {
        var remixError = ValidateRemixState(policy, remix, remixUrl);
        if (remixError != null)
        {
            return remixError;
        }

        var descriptionError = ValidateDescription(policy, description);
        if (descriptionError != null)
        {
            return descriptionError;
        }

        var licenseError = ValidateLicense(policy, license, parsedLicense);
        if (licenseError != null)
        {
            return licenseError;
        }

        if (nsfw && !policy.AllowNsfwContent)
        {
            return "NSFW content is not allowed on this site.";
        }

        if (aiGenerated && !policy.AllowAiGeneratedContent)
        {
            return "AI-generated content is not allowed on this site.";
        }

        if (policy.RequireThumbnail && !BundleContainsImage(files))
        {
            return "At least one image file is required for the model thumbnail.";
        }

        return null;
    }

    public string? ValidateUpdateDescription(ModelMetadataPolicySnapshot policy, string description) =>
        ValidateDescription(policy, description);

    public string? ValidateUpdateLicense(
        ModelMetadataPolicySnapshot policy,
        string? licenseRaw,
        LicenseTypes? license) =>
        ValidateLicense(policy, licenseRaw, license);

    public string? ValidateUpdateNsfw(ModelMetadataPolicySnapshot policy, bool nsfw) =>
        nsfw && !policy.AllowNsfwContent ? "NSFW content is not allowed on this site." : null;

    public string? ValidateUpdateAiGenerated(ModelMetadataPolicySnapshot policy, bool aiGenerated) =>
        aiGenerated && !policy.AllowAiGeneratedContent
            ? "AI-generated content is not allowed on this site."
            : null;

    public string? ValidateUpdateRemix(
        ModelMetadataPolicySnapshot policy,
        bool currentIsRemix,
        string? currentRemixUrl,
        bool? requestedIsRemix,
        string? requestedRemixUrl)
    {
        if (requestedIsRemix == false)
        {
            return null;
        }

        var effectiveIsRemix = requestedIsRemix ?? currentIsRemix;
        if (!effectiveIsRemix)
        {
            return null;
        }

        if (!policy.AllowModelRemixing)
        {
            return "Model remixing is not allowed on this site.";
        }

        var effectiveUrl = requestedRemixUrl ?? currentRemixUrl;
        if (requestedRemixUrl != null && string.IsNullOrWhiteSpace(requestedRemixUrl))
        {
            effectiveUrl = null;
        }

        return ValidateRemixAttribution(policy, effectiveUrl);
    }

    public string? ValidateAddTag(ModelMetadataPolicySnapshot policy, int currentTagCount, bool isNewLink)
    {
        if (!isNewLink)
        {
            return null;
        }

        if (policy.MaxTagsPerModel > 0 && currentTagCount >= policy.MaxTagsPerModel)
        {
            return $"A model cannot have more than {policy.MaxTagsPerModel} tags.";
        }

        return null;
    }

    public string? ValidateRemoveTag(ModelMetadataPolicySnapshot policy, int currentTagCount)
    {
        if (policy.RequireModelTags && currentTagCount <= 1)
        {
            return "At least one tag is required on this model.";
        }

        return null;
    }

    public string? ValidateAddCategory(ModelMetadataPolicySnapshot policy, int currentCategoryCount)
    {
        if (policy.MaxCategoriesPerModel > 0 && currentCategoryCount >= policy.MaxCategoriesPerModel)
        {
            return $"A model cannot have more than {policy.MaxCategoriesPerModel} categories.";
        }

        return null;
    }

    public string? ValidateRemoveCategory(ModelMetadataPolicySnapshot policy, int currentCategoryCount)
    {
        if (policy.RequireCategorySelection && currentCategoryCount <= 1)
        {
            return "At least one category is required on this model.";
        }

        return null;
    }

    private static string? ValidateDescription(ModelMetadataPolicySnapshot policy, string? description)
    {
        var text = description ?? string.Empty;
        var trimmed = text.Trim();
        var length = trimmed.Length;

        if (policy.RequireModelDescription && length == 0)
        {
            return "Model description is required.";
        }

        if (length > 0 && policy.MinDescriptionLength > 0 && length < policy.MinDescriptionLength)
        {
            return $"Description must be at least {policy.MinDescriptionLength} characters.";
        }

        if (policy.MaxDescriptionLength > 0 && text.Length > policy.MaxDescriptionLength)
        {
            return $"Description cannot exceed {policy.MaxDescriptionLength} characters.";
        }

        return null;
    }

    private static string? ValidateLicense(
        ModelMetadataPolicySnapshot policy,
        string? licenseRaw,
        LicenseTypes? parsedLicense)
    {
        if (!policy.AllowCustomLicenses && ModelLicenseParser.IsCustomLicenseRequest(licenseRaw))
        {
            return "Custom licenses are not allowed on this site.";
        }

        if (policy.RequireLicenseSelection && parsedLicense == null)
        {
            return "A license must be selected.";
        }

        return null;
    }

    private static string? ValidateRemixState(ModelMetadataPolicySnapshot policy, bool remix, string? remixUrl)
    {
        if (!remix)
        {
            return null;
        }

        if (!policy.AllowModelRemixing)
        {
            return "Model remixing is not allowed on this site.";
        }

        return ValidateRemixAttribution(policy, remixUrl);
    }

    private static string? ValidateRemixAttribution(ModelMetadataPolicySnapshot policy, string? remixUrl)
    {
        if (!policy.RequireRemixAttribution)
        {
            return null;
        }

        var trimmed = remixUrl?.Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            return "Remix URL is required when marking a model as a remix.";
        }

        if (!Uri.TryCreate(trimmed, UriKind.Absolute, out _))
        {
            return "Remix URL must be a valid absolute URL.";
        }

        return null;
    }

    private static bool BundleContainsImage(IFormFile[] files)
    {
        foreach (var file in files)
        {
            if (file.Length == 0)
            {
                continue;
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (ImageExtensions.Contains(extension))
            {
                return true;
            }
        }

        return false;
    }
}