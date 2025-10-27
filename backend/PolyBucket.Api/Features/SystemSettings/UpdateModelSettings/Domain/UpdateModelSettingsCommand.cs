using System.ComponentModel.DataAnnotations;
using MediatR;
using PolyBucket.Api.Features.Models.Shared.Domain.Enums;

namespace PolyBucket.Api.Features.SystemSettings.UpdateModelSettings.Domain
{
    public class UpdateModelSettingsCommand : IRequest<UpdateModelSettingsResponse>
    {
        public bool AllowAnonUploads { get; set; } = false;
        public bool RequireUploadModeration { get; set; } = true;
        
        [Required(ErrorMessage = "Default privacy setting is required")]
        public string DefaultPrivacySetting { get; set; } = "Public";
        
        public bool AllowAnonDownloads { get; set; } = true;
        public bool EnableModelVersioning { get; set; } = true;
        
        [Range(1, 100000, ErrorMessage = "Limit total models must be between 1 and 100000")]
        public int LimitTotalModels { get; set; } = 1000;
        
        public bool AllowNSFWContent { get; set; } = false;
        public bool AllowAIGeneratedContent { get; set; } = true;
        public bool RequireModelDescription { get; set; } = true;
        public bool RequireModelTags { get; set; } = false;
        
        [Range(0, 1000, ErrorMessage = "Min description length must be between 0 and 1000")]
        public int MinDescriptionLength { get; set; } = 10;
        
        [Range(10, 10000, ErrorMessage = "Max description length must be between 10 and 10000")]
        public int MaxDescriptionLength { get; set; } = 2000;
        
        [Range(0, 100, ErrorMessage = "Max tags per model must be between 0 and 100")]
        public int MaxTagsPerModel { get; set; } = 10;
        
        public bool AutoApproveVerifiedUsers { get; set; } = false;
        public bool RequireThumbnail { get; set; } = false;
        public bool AllowModelRemixing { get; set; } = true;
        public bool RequireRemixAttribution { get; set; } = true;
        
        [Range(1, 10000, ErrorMessage = "Max models per user must be between 1 and 10000")]
        public int MaxModelsPerUser { get; set; } = 100;
        
        public bool EnableModelComments { get; set; } = true;
        public bool EnableModelLikes { get; set; } = true;
        public bool EnableModelDownloads { get; set; } = true;
        public bool RequireLicenseSelection { get; set; } = true;
        public bool AllowCustomLicenses { get; set; } = false;
        public bool EnableModelCollections { get; set; } = true;
        public bool RequireCategorySelection { get; set; } = false;
        
        [Range(0, 20, ErrorMessage = "Max categories per model must be between 0 and 20")]
        public int MaxCategoriesPerModel { get; set; } = 3;
        
        public bool EnableModelSharing { get; set; } = true;
        public bool EnableModelEmbedding { get; set; } = true;
        public bool RequireModelPreview { get; set; } = false;
        public bool AutoGenerateModelPreviews { get; set; } = true;
        public bool EnableModelAnalytics { get; set; } = true;
        public bool RequireUserAgreement { get; set; } = false;
        
        [StringLength(10000, ErrorMessage = "User agreement text cannot exceed 10000 characters")]
        public string UserAgreementText { get; set; } = string.Empty;
        
        public bool EnableModelExport { get; set; } = true;
        public bool EnableModelImport { get; set; } = false;
        public bool RequireModelValidation { get; set; } = false;
        public bool EnableModelBackup { get; set; } = true;
        
        [Range(1, 3650, ErrorMessage = "Model backup retention days must be between 1 and 3650")]
        public int ModelBackupRetentionDays { get; set; } = 90;
        
        public bool EnableModelArchiving { get; set; } = true;
        
        [Range(1, 3650, ErrorMessage = "Model archive threshold days must be between 1 and 3650")]
        public int ModelArchiveThresholdDays { get; set; } = 365;
        
        public bool RequireModeratorApproval { get; set; } = true;
        public bool EnableAutoModeration { get; set; } = false;
        public bool RequireContentRating { get; set; } = false;
        public bool EnableModelFlagging { get; set; } = true;
        public bool RequireFlagReason { get; set; } = true;
        public bool EnableModelReporting { get; set; } = true;
        public bool RequireReportDetails { get; set; } = true;
        public bool EnableModelBlocking { get; set; } = true;
        public bool RequireBlockReason { get; set; } = true;
        public bool EnableModelWhitelisting { get; set; } = false;
        public bool EnableModelBlacklisting { get; set; } = false;
        public bool RequireModelApproval { get; set; } = true;
        public bool EnableModelRejection { get; set; } = true;
        public bool RequireRejectionReason { get; set; } = true;
        public bool EnableModelAppeals { get; set; } = true;
        public bool RequireAppealDetails { get; set; } = true;
        public bool EnableModelLocking { get; set; } = true;
        public bool RequireLockReason { get; set; } = true;
        public bool EnableModelUnlocking { get; set; } = true;
        public bool RequireUnlockApproval { get; set; } = true;
        public bool EnableModelDeletion { get; set; } = true;
        public bool RequireDeletionApproval { get; set; } = true;
        public bool RequireDeletionReason { get; set; } = true;
        public bool EnableModelRestoration { get; set; } = true;
        public bool RequireRestorationApproval { get; set; } = true;

        public bool IsValid(out List<ValidationResult> validationResults)
        {
            validationResults = new List<ValidationResult>();
            var context = new ValidationContext(this);
            
            // Validate DefaultPrivacySetting enum value
            if (!Enum.TryParse<PrivacySettings>(DefaultPrivacySetting, true, out _))
            {
                validationResults.Add(new ValidationResult("Invalid default privacy setting value. Must be one of: Public, Private, Unlisted", new[] { nameof(DefaultPrivacySetting) }));
            }
            
            // Validate MinDescriptionLength <= MaxDescriptionLength
            if (MinDescriptionLength > MaxDescriptionLength)
            {
                validationResults.Add(new ValidationResult("Min description length cannot be greater than max description length", new[] { nameof(MinDescriptionLength), nameof(MaxDescriptionLength) }));
            }
            
            return validationResults.Count == 0;
        }
    }

    public class UpdateModelSettingsResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
