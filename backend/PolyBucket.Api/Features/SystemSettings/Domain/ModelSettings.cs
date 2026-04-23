using PolyBucket.Api.Common.Entities;
using PolyBucket.Api.Common.Models.Enums;

namespace PolyBucket.Api.Features.SystemSettings.Domain
{
    public class ModelSettings : Auditable
    {
        public bool AllowAnonUploads { get; set; } = false;
        public bool RequireUploadModeration { get; set; } = true;
        public PrivacySettings DefaultPrivacySetting { get; set; } = PrivacySettings.Public;
        public bool AllowAnonDownloads { get; set; } = true;
        public bool EnableModelVersioning { get; set; } = true;
        public int LimitTotalModels { get; set; } = 1000;
        
        // Additional model-specific settings
        public bool AllowNSFWContent { get; set; } = false;
        public bool AllowAIGeneratedContent { get; set; } = true;
        public bool RequireModelDescription { get; set; } = true;
        public bool RequireModelTags { get; set; } = false;
        public int MinDescriptionLength { get; set; } = 10;
        public int MaxDescriptionLength { get; set; } = 2000;
        public int MaxTagsPerModel { get; set; } = 10;
        public bool AutoApproveVerifiedUsers { get; set; } = false;
        public bool RequireThumbnail { get; set; } = false;
        public bool AllowModelRemixing { get; set; } = true;
        public bool RequireRemixAttribution { get; set; } = true;
        public int MaxModelsPerUser { get; set; } = 100;
        public bool EnableModelComments { get; set; } = true;
        public bool EnableModelLikes { get; set; } = true;
        public bool EnableModelDownloads { get; set; } = true;
        public bool RequireLicenseSelection { get; set; } = true;
        public bool AllowCustomLicenses { get; set; } = false;
        public bool EnableModelCollections { get; set; } = true;
        public bool RequireCategorySelection { get; set; } = false;
        public int MaxCategoriesPerModel { get; set; } = 3;
        public bool EnableModelSharing { get; set; } = true;
        public bool EnableModelEmbedding { get; set; } = true;
        public bool RequireModelPreview { get; set; } = false;
        public bool AutoGenerateModelPreviews { get; set; } = true;
        public bool EnableModelAnalytics { get; set; } = true;
        public bool RequireUserAgreement { get; set; } = false;
        public string UserAgreementText { get; set; } = string.Empty;
        public bool EnableModelExport { get; set; } = true;
        public bool EnableModelImport { get; set; } = false;
        public bool RequireModelValidation { get; set; } = false;
        public bool EnableModelBackup { get; set; } = true;
        public int ModelBackupRetentionDays { get; set; } = 90;
        public bool EnableModelArchiving { get; set; } = true;
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
    }
}
