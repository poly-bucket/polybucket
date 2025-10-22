using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.SystemSettings.Domain;
using Microsoft.Extensions.Logging;

namespace PolyBucket.Api.Seeders
{
    public class ModelSettingsSeeder(PolyBucketDbContext context, ILogger<ModelSettingsSeeder> logger)
    {
        private readonly PolyBucketDbContext _context = context;
        private readonly ILogger<ModelSettingsSeeder> _logger = logger;

        public async Task SeedAsync()
        {
            if (await _context.ModelSettings.AnyAsync())
            {
                _logger.LogInformation("Model settings already exist, skipping seeding");
                return;
            }

            var adminUser = await _context.Users
                .Where(u => u.Role.Name == "Admin")
                .FirstOrDefaultAsync();

            if (adminUser == null)
            {
                throw new InvalidOperationException("Admin user not found. Please ensure admin user is created before seeding model settings.");
            }

            var modelSettings = new ModelSettings
            {
                Id = Guid.NewGuid(),
                AllowAnonUploads = false,
                RequireUploadModeration = true,
                DefaultPrivacySetting = PolyBucket.Api.Features.Models.Domain.Enums.PrivacySettings.Public,
                AllowAnonDownloads = true,
                EnableModelVersioning = true,
                LimitTotalModels = 1000,
                AllowNSFWContent = false,
                AllowAIGeneratedContent = true,
                RequireModelDescription = true,
                RequireModelTags = false,
                MinDescriptionLength = 10,
                MaxDescriptionLength = 2000,
                MaxTagsPerModel = 10,
                AutoApproveVerifiedUsers = false,
                RequireThumbnail = false,
                AllowModelRemixing = true,
                RequireRemixAttribution = true,
                MaxModelsPerUser = 100,
                EnableModelComments = true,
                EnableModelLikes = true,
                EnableModelDownloads = true,
                RequireLicenseSelection = true,
                AllowCustomLicenses = false,
                EnableModelCollections = true,
                RequireCategorySelection = false,
                MaxCategoriesPerModel = 3,
                EnableModelSharing = true,
                EnableModelEmbedding = true,
                RequireModelPreview = false,
                AutoGenerateModelPreviews = true,
                EnableModelAnalytics = true,
                RequireUserAgreement = false,
                UserAgreementText = string.Empty,
                EnableModelExport = true,
                EnableModelImport = false,
                RequireModelValidation = false,
                EnableModelBackup = true,
                ModelBackupRetentionDays = 90,
                EnableModelArchiving = true,
                ModelArchiveThresholdDays = 365,
                RequireModeratorApproval = true,
                EnableAutoModeration = false,
                RequireContentRating = false,
                EnableModelFlagging = true,
                RequireFlagReason = true,
                EnableModelReporting = true,
                RequireReportDetails = true,
                EnableModelBlocking = true,
                RequireBlockReason = true,
                EnableModelWhitelisting = false,
                EnableModelBlacklisting = false,
                RequireModelApproval = true,
                EnableModelRejection = true,
                RequireRejectionReason = true,
                EnableModelAppeals = true,
                RequireAppealDetails = true,
                EnableModelLocking = true,
                RequireLockReason = true,
                EnableModelUnlocking = true,
                RequireUnlockApproval = true,
                EnableModelDeletion = true,
                RequireDeletionApproval = true,
                RequireDeletionReason = true,
                EnableModelRestoration = true,
                RequireRestorationApproval = true,
                CreatedAt = DateTime.UtcNow,
                CreatedById = adminUser.Id,
                UpdatedAt = DateTime.UtcNow,
                UpdatedById = adminUser.Id
            };

            _context.ModelSettings.Add(modelSettings);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Model settings seeded successfully");
        }
    }
}
