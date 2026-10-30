using PolyBucket.Api.Features.SystemSettings.UpdateModelConfigurationSettings.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PolyBucket.Database.Persistence;
using PolyBucket.Api.Features.SystemSettings.UpdateModelConfigurationSettings.Domain;
using PolyBucket.Database.Entities.Models.Enums;
using System.Security.Claims;

namespace PolyBucket.Api.Features.SystemSettings.UpdateModelConfigurationSettings.Domain
{
    public class UpdateModelConfigurationSettingsService(
        IUpdateModelConfigurationSettingsRepository repository,
        ILogger<UpdateModelConfigurationSettingsService> logger,
        IHttpContextAccessor httpContextAccessor) : IUpdateModelConfigurationSettingsService
    {
        private readonly IUpdateModelConfigurationSettingsRepository _repository = repository;
        private readonly ILogger<UpdateModelConfigurationSettingsService> _logger = logger;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

        public async Task<UpdateModelConfigurationSettingsResponse> UpdateModelConfigurationSettingsAsync(UpdateModelConfigurationSettingsCommand request, CancellationToken cancellationToken = default)
        {
            try
            {
                var modelSettings = await _repository.Context.ModelSettings.FirstOrDefaultAsync(cancellationToken);
                
                if (modelSettings == null)
                {
                    return new UpdateModelConfigurationSettingsResponse
                    {
                        Success = false,
                        Message = "Model settings not found. Please complete first-time setup first."
                    };
                }

                // Parse and validate the privacy settings enum
                if (!Enum.TryParse<PrivacySettings>(request.DefaultPrivacySetting, true, out var privacySettings))
                {
                    return new UpdateModelConfigurationSettingsResponse
                    {
                        Success = false,
                        Message = "Invalid default privacy setting value"
                    };
                }

                // Update model settings
                modelSettings.AllowAnonUploads = request.AllowAnonUploads;
                modelSettings.RequireUploadModeration = request.RequireUploadModeration;
                modelSettings.DefaultPrivacySetting = privacySettings;
                modelSettings.AllowAnonDownloads = request.AllowAnonDownloads;
                modelSettings.EnableModelVersioning = request.EnableModelVersioning;
                modelSettings.LimitTotalModels = request.LimitTotalModels;
                modelSettings.AllowNSFWContent = request.AllowNSFWContent;
                modelSettings.AllowAIGeneratedContent = request.AllowAIGeneratedContent;
                modelSettings.RequireModelDescription = request.RequireModelDescription;
                modelSettings.RequireModelTags = request.RequireModelTags;
                modelSettings.MinDescriptionLength = request.MinDescriptionLength;
                modelSettings.MaxDescriptionLength = request.MaxDescriptionLength;
                modelSettings.MaxTagsPerModel = request.MaxTagsPerModel;
                modelSettings.AutoApproveVerifiedUsers = request.AutoApproveVerifiedUsers;
                modelSettings.RequireThumbnail = request.RequireThumbnail;
                modelSettings.AllowModelRemixing = request.AllowModelRemixing;
                modelSettings.RequireRemixAttribution = request.RequireRemixAttribution;
                modelSettings.MaxModelsPerUser = request.MaxModelsPerUser;
                modelSettings.EnableModelComments = request.EnableModelComments;
                modelSettings.EnableModelLikes = request.EnableModelLikes;
                modelSettings.EnableModelDownloads = request.EnableModelDownloads;
                modelSettings.RequireLicenseSelection = request.RequireLicenseSelection;
                modelSettings.AllowCustomLicenses = request.AllowCustomLicenses;
                modelSettings.EnableModelCollections = request.EnableModelCollections;
                modelSettings.RequireCategorySelection = request.RequireCategorySelection;
                modelSettings.MaxCategoriesPerModel = request.MaxCategoriesPerModel;
                modelSettings.EnableModelSharing = request.EnableModelSharing;
                modelSettings.EnableModelEmbedding = request.EnableModelEmbedding;
                modelSettings.RequireModelPreview = request.RequireModelPreview;
                modelSettings.AutoGenerateModelPreviews = request.AutoGenerateModelPreviews;
                modelSettings.EnableModelAnalytics = request.EnableModelAnalytics;
                modelSettings.RequireUserAgreement = request.RequireUserAgreement;
                modelSettings.UserAgreementText = request.UserAgreementText;
                modelSettings.EnableModelExport = request.EnableModelExport;
                modelSettings.EnableModelImport = request.EnableModelImport;
                modelSettings.RequireModelValidation = request.RequireModelValidation;
                modelSettings.EnableModelBackup = request.EnableModelBackup;
                modelSettings.ModelBackupRetentionDays = request.ModelBackupRetentionDays;
                modelSettings.EnableModelArchiving = request.EnableModelArchiving;
                modelSettings.ModelArchiveThresholdDays = request.ModelArchiveThresholdDays;
                modelSettings.RequireModeratorApproval = request.RequireModeratorApproval;
                modelSettings.EnableAutoModeration = request.EnableAutoModeration;
                modelSettings.RequireContentRating = request.RequireContentRating;
                modelSettings.EnableModelFlagging = request.EnableModelFlagging;
                modelSettings.RequireFlagReason = request.RequireFlagReason;
                modelSettings.EnableModelReporting = request.EnableModelReporting;
                modelSettings.RequireReportDetails = request.RequireReportDetails;
                modelSettings.EnableModelBlocking = request.EnableModelBlocking;
                modelSettings.RequireBlockReason = request.RequireBlockReason;
                modelSettings.EnableModelWhitelisting = request.EnableModelWhitelisting;
                modelSettings.EnableModelBlacklisting = request.EnableModelBlacklisting;
                modelSettings.RequireModelApproval = request.RequireModelApproval;
                modelSettings.EnableModelRejection = request.EnableModelRejection;
                modelSettings.RequireRejectionReason = request.RequireRejectionReason;
                modelSettings.EnableModelAppeals = request.EnableModelAppeals;
                modelSettings.RequireAppealDetails = request.RequireAppealDetails;
                modelSettings.EnableModelLocking = request.EnableModelLocking;
                modelSettings.RequireLockReason = request.RequireLockReason;
                modelSettings.EnableModelUnlocking = request.EnableModelUnlocking;
                modelSettings.RequireUnlockApproval = request.RequireUnlockApproval;
                modelSettings.EnableModelDeletion = request.EnableModelDeletion;
                modelSettings.RequireDeletionApproval = request.RequireDeletionApproval;
                modelSettings.RequireDeletionReason = request.RequireDeletionReason;
                modelSettings.EnableModelRestoration = request.EnableModelRestoration;
                modelSettings.RequireRestorationApproval = request.RequireRestorationApproval;
                
                // Update timestamps
                modelSettings.UpdatedAt = DateTime.UtcNow;
                
                // Get current user ID if available
                var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
                {
                    modelSettings.UpdatedById = userId;
                }

                await _repository.Context.SaveChangesAsync(cancellationToken);
                
                _logger.LogInformation("Model settings updated successfully by user {UserId}", 
                    userIdClaim?.Value ?? "unknown");

                return new UpdateModelConfigurationSettingsResponse
                {
                    Success = true,
                    Message = "Model settings updated successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update model settings");
                return new UpdateModelConfigurationSettingsResponse
                {
                    Success = false,
                    Message = "Failed to update model settings. Please try again."
                };
            }
        }
    }
}
