using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.SystemSettings.GetModelSettings.Domain;

namespace PolyBucket.Api.Features.SystemSettings.GetModelSettings.Domain
{
    public class GetModelSettingsQueryHandler(
        PolyBucketDbContext context,
        ILogger<GetModelSettingsQueryHandler> logger) : IRequestHandler<GetModelSettingsQuery, GetModelSettingsResponse>
    {
        private readonly PolyBucketDbContext _context = context;
        private readonly ILogger<GetModelSettingsQueryHandler> _logger = logger;

        public async Task<GetModelSettingsResponse> Handle(GetModelSettingsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var modelSettings = await _context.ModelSettings.FirstOrDefaultAsync(cancellationToken);
                
                if (modelSettings == null)
                {
                    return new GetModelSettingsResponse
                    {
                        Success = false,
                        Message = "Model settings not found. Please complete first-time setup first."
                    };
                }

                var settings = new ModelSettingsData
                {
                    AllowAnonUploads = modelSettings.AllowAnonUploads,
                    RequireUploadModeration = modelSettings.RequireUploadModeration,
                    DefaultPrivacySetting = modelSettings.DefaultPrivacySetting.ToString(),
                    AllowAnonDownloads = modelSettings.AllowAnonDownloads,
                    EnableModelVersioning = modelSettings.EnableModelVersioning,
                    LimitTotalModels = modelSettings.LimitTotalModels,
                    AllowNSFWContent = modelSettings.AllowNSFWContent,
                    AllowAIGeneratedContent = modelSettings.AllowAIGeneratedContent,
                    RequireModelDescription = modelSettings.RequireModelDescription,
                    RequireModelTags = modelSettings.RequireModelTags,
                    MinDescriptionLength = modelSettings.MinDescriptionLength,
                    MaxDescriptionLength = modelSettings.MaxDescriptionLength,
                    MaxTagsPerModel = modelSettings.MaxTagsPerModel,
                    AutoApproveVerifiedUsers = modelSettings.AutoApproveVerifiedUsers,
                    RequireThumbnail = modelSettings.RequireThumbnail,
                    AllowModelRemixing = modelSettings.AllowModelRemixing,
                    RequireRemixAttribution = modelSettings.RequireRemixAttribution,
                    MaxModelsPerUser = modelSettings.MaxModelsPerUser,
                    EnableModelComments = modelSettings.EnableModelComments,
                    EnableModelLikes = modelSettings.EnableModelLikes,
                    EnableModelDownloads = modelSettings.EnableModelDownloads,
                    RequireLicenseSelection = modelSettings.RequireLicenseSelection,
                    AllowCustomLicenses = modelSettings.AllowCustomLicenses,
                    EnableModelCollections = modelSettings.EnableModelCollections,
                    RequireCategorySelection = modelSettings.RequireCategorySelection,
                    MaxCategoriesPerModel = modelSettings.MaxCategoriesPerModel,
                    EnableModelSharing = modelSettings.EnableModelSharing,
                    EnableModelEmbedding = modelSettings.EnableModelEmbedding,
                    RequireModelPreview = modelSettings.RequireModelPreview,
                    AutoGenerateModelPreviews = modelSettings.AutoGenerateModelPreviews,
                    EnableModelAnalytics = modelSettings.EnableModelAnalytics,
                    RequireUserAgreement = modelSettings.RequireUserAgreement,
                    UserAgreementText = modelSettings.UserAgreementText,
                    EnableModelExport = modelSettings.EnableModelExport,
                    EnableModelImport = modelSettings.EnableModelImport,
                    RequireModelValidation = modelSettings.RequireModelValidation,
                    EnableModelBackup = modelSettings.EnableModelBackup,
                    ModelBackupRetentionDays = modelSettings.ModelBackupRetentionDays,
                    EnableModelArchiving = modelSettings.EnableModelArchiving,
                    ModelArchiveThresholdDays = modelSettings.ModelArchiveThresholdDays,
                    RequireModeratorApproval = modelSettings.RequireModeratorApproval,
                    EnableAutoModeration = modelSettings.EnableAutoModeration,
                    RequireContentRating = modelSettings.RequireContentRating,
                    EnableModelFlagging = modelSettings.EnableModelFlagging,
                    RequireFlagReason = modelSettings.RequireFlagReason,
                    EnableModelReporting = modelSettings.EnableModelReporting,
                    RequireReportDetails = modelSettings.RequireReportDetails,
                    EnableModelBlocking = modelSettings.EnableModelBlocking,
                    RequireBlockReason = modelSettings.RequireBlockReason,
                    EnableModelWhitelisting = modelSettings.EnableModelWhitelisting,
                    EnableModelBlacklisting = modelSettings.EnableModelBlacklisting,
                    RequireModelApproval = modelSettings.RequireModelApproval,
                    EnableModelRejection = modelSettings.EnableModelRejection,
                    RequireRejectionReason = modelSettings.RequireRejectionReason,
                    EnableModelAppeals = modelSettings.EnableModelAppeals,
                    RequireAppealDetails = modelSettings.RequireAppealDetails,
                    EnableModelLocking = modelSettings.EnableModelLocking,
                    RequireLockReason = modelSettings.RequireLockReason,
                    EnableModelUnlocking = modelSettings.EnableModelUnlocking,
                    RequireUnlockApproval = modelSettings.RequireUnlockApproval,
                    EnableModelDeletion = modelSettings.EnableModelDeletion,
                    RequireDeletionApproval = modelSettings.RequireDeletionApproval,
                    RequireDeletionReason = modelSettings.RequireDeletionReason,
                    EnableModelRestoration = modelSettings.EnableModelRestoration,
                    RequireRestorationApproval = modelSettings.RequireRestorationApproval
                };

                return new GetModelSettingsResponse
                {
                    Success = true,
                    Message = "Model settings retrieved successfully",
                    Settings = settings
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve model settings");
                return new GetModelSettingsResponse
                {
                    Success = false,
                    Message = "Failed to retrieve model settings. Please try again."
                };
            }
        }
    }
}
