using Microsoft.AspNetCore.Http;
using PolyBucket.Api.Features.SystemSettings.Services;
using PolyBucket.Database.Entities.Models.Enums;

namespace PolyBucket.Api.Features.Models.UploadValidation.Domain;

public interface IModelMetadataValidator
{
    string? ValidateCreateMetadata(
        ModelMetadataPolicySnapshot policy,
        string? description,
        string? license,
        LicenseTypes? parsedLicense,
        bool nsfw,
        bool aiGenerated,
        bool remix,
        string? remixUrl,
        IFormFile[] files);

    string? ValidateUpdateRemix(
        ModelMetadataPolicySnapshot policy,
        bool currentIsRemix,
        string? currentRemixUrl,
        bool? requestedIsRemix,
        string? requestedRemixUrl);

    string? ValidateUpdateDescription(ModelMetadataPolicySnapshot policy, string description);

    string? ValidateUpdateLicense(
        ModelMetadataPolicySnapshot policy,
        string? licenseRaw,
        LicenseTypes? license);

    string? ValidateUpdateNsfw(ModelMetadataPolicySnapshot policy, bool nsfw);

    string? ValidateUpdateAiGenerated(ModelMetadataPolicySnapshot policy, bool aiGenerated);

    string? ValidateAddTag(ModelMetadataPolicySnapshot policy, int currentTagCount, bool isNewLink);

    string? ValidateRemoveTag(ModelMetadataPolicySnapshot policy, int currentTagCount);

    string? ValidateAddCategory(ModelMetadataPolicySnapshot policy, int currentCategoryCount);

    string? ValidateRemoveCategory(ModelMetadataPolicySnapshot policy, int currentCategoryCount);
}