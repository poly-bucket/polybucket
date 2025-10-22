using MediatR;

namespace PolyBucket.Api.Features.SystemSettings.GetModelSettings.Domain
{
    public class GetModelSettingsQuery : IRequest<GetModelSettingsResponse>
    {
    }

    public class GetModelSettingsResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public ModelSettingsData? Settings { get; set; }
    }

    public class ModelSettingsData
    {
        public bool AllowAnonUploads { get; set; }
        public bool RequireUploadModeration { get; set; }
        public string DefaultPrivacySetting { get; set; } = "Public";
        public bool AllowAnonDownloads { get; set; }
        public bool EnableModelVersioning { get; set; }
        public int LimitTotalModels { get; set; }
        public bool AllowNSFWContent { get; set; }
        public bool AllowAIGeneratedContent { get; set; }
        public bool RequireModelDescription { get; set; }
        public bool RequireModelTags { get; set; }
        public int MinDescriptionLength { get; set; }
        public int MaxDescriptionLength { get; set; }
        public int MaxTagsPerModel { get; set; }
        public bool AutoApproveVerifiedUsers { get; set; }
        public bool RequireThumbnail { get; set; }
        public bool AllowModelRemixing { get; set; }
        public bool RequireRemixAttribution { get; set; }
        public int MaxModelsPerUser { get; set; }
        public bool EnableModelComments { get; set; }
        public bool EnableModelLikes { get; set; }
        public bool EnableModelDownloads { get; set; }
        public bool RequireLicenseSelection { get; set; }
        public bool AllowCustomLicenses { get; set; }
        public bool EnableModelCollections { get; set; }
        public bool RequireCategorySelection { get; set; }
        public int MaxCategoriesPerModel { get; set; }
        public bool EnableModelSharing { get; set; }
        public bool EnableModelEmbedding { get; set; }
        public bool RequireModelPreview { get; set; }
        public bool AutoGenerateModelPreviews { get; set; }
        public bool EnableModelAnalytics { get; set; }
        public bool RequireUserAgreement { get; set; }
        public string UserAgreementText { get; set; } = string.Empty;
        public bool EnableModelExport { get; set; }
        public bool EnableModelImport { get; set; }
        public bool RequireModelValidation { get; set; }
        public bool EnableModelBackup { get; set; }
        public int ModelBackupRetentionDays { get; set; }
        public bool EnableModelArchiving { get; set; }
        public int ModelArchiveThresholdDays { get; set; }
        public bool RequireModeratorApproval { get; set; }
        public bool EnableAutoModeration { get; set; }
        public bool RequireContentRating { get; set; }
        public bool EnableModelFlagging { get; set; }
        public bool RequireFlagReason { get; set; }
        public bool EnableModelReporting { get; set; }
        public bool RequireReportDetails { get; set; }
        public bool EnableModelBlocking { get; set; }
        public bool RequireBlockReason { get; set; }
        public bool EnableModelWhitelisting { get; set; }
        public bool EnableModelBlacklisting { get; set; }
        public bool RequireModelApproval { get; set; }
        public bool EnableModelRejection { get; set; }
        public bool RequireRejectionReason { get; set; }
        public bool EnableModelAppeals { get; set; }
        public bool RequireAppealDetails { get; set; }
        public bool EnableModelLocking { get; set; }
        public bool RequireLockReason { get; set; }
        public bool EnableModelUnlocking { get; set; }
        public bool RequireUnlockApproval { get; set; }
        public bool EnableModelDeletion { get; set; }
        public bool RequireDeletionApproval { get; set; }
        public bool RequireDeletionReason { get; set; }
        public bool EnableModelRestoration { get; set; }
        public bool RequireRestorationApproval { get; set; }
    }
}
