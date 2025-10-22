using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class AddModelSettingsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ModelSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AllowAnonUploads = table.Column<bool>(type: "boolean", nullable: false),
                    RequireUploadModeration = table.Column<bool>(type: "boolean", nullable: false),
                    DefaultPrivacySetting = table.Column<int>(type: "integer", nullable: false),
                    AllowAnonDownloads = table.Column<bool>(type: "boolean", nullable: false),
                    EnableModelVersioning = table.Column<bool>(type: "boolean", nullable: false),
                    LimitTotalModels = table.Column<int>(type: "integer", nullable: false),
                    AllowNSFWContent = table.Column<bool>(type: "boolean", nullable: false),
                    AllowAIGeneratedContent = table.Column<bool>(type: "boolean", nullable: false),
                    RequireModelDescription = table.Column<bool>(type: "boolean", nullable: false),
                    RequireModelTags = table.Column<bool>(type: "boolean", nullable: false),
                    MinDescriptionLength = table.Column<int>(type: "integer", nullable: false),
                    MaxDescriptionLength = table.Column<int>(type: "integer", nullable: false),
                    MaxTagsPerModel = table.Column<int>(type: "integer", nullable: false),
                    AutoApproveVerifiedUsers = table.Column<bool>(type: "boolean", nullable: false),
                    RequireThumbnail = table.Column<bool>(type: "boolean", nullable: false),
                    AllowModelRemixing = table.Column<bool>(type: "boolean", nullable: false),
                    RequireRemixAttribution = table.Column<bool>(type: "boolean", nullable: false),
                    MaxModelsPerUser = table.Column<int>(type: "integer", nullable: false),
                    EnableModelComments = table.Column<bool>(type: "boolean", nullable: false),
                    EnableModelLikes = table.Column<bool>(type: "boolean", nullable: false),
                    EnableModelDownloads = table.Column<bool>(type: "boolean", nullable: false),
                    RequireLicenseSelection = table.Column<bool>(type: "boolean", nullable: false),
                    AllowCustomLicenses = table.Column<bool>(type: "boolean", nullable: false),
                    EnableModelCollections = table.Column<bool>(type: "boolean", nullable: false),
                    RequireCategorySelection = table.Column<bool>(type: "boolean", nullable: false),
                    MaxCategoriesPerModel = table.Column<int>(type: "integer", nullable: false),
                    EnableModelSharing = table.Column<bool>(type: "boolean", nullable: false),
                    EnableModelEmbedding = table.Column<bool>(type: "boolean", nullable: false),
                    RequireModelPreview = table.Column<bool>(type: "boolean", nullable: false),
                    AutoGenerateModelPreviews = table.Column<bool>(type: "boolean", nullable: false),
                    EnableModelAnalytics = table.Column<bool>(type: "boolean", nullable: false),
                    RequireUserAgreement = table.Column<bool>(type: "boolean", nullable: false),
                    UserAgreementText = table.Column<string>(type: "text", nullable: false),
                    EnableModelExport = table.Column<bool>(type: "boolean", nullable: false),
                    EnableModelImport = table.Column<bool>(type: "boolean", nullable: false),
                    RequireModelValidation = table.Column<bool>(type: "boolean", nullable: false),
                    EnableModelBackup = table.Column<bool>(type: "boolean", nullable: false),
                    ModelBackupRetentionDays = table.Column<int>(type: "integer", nullable: false),
                    EnableModelArchiving = table.Column<bool>(type: "boolean", nullable: false),
                    ModelArchiveThresholdDays = table.Column<int>(type: "integer", nullable: false),
                    RequireModeratorApproval = table.Column<bool>(type: "boolean", nullable: false),
                    EnableAutoModeration = table.Column<bool>(type: "boolean", nullable: false),
                    RequireContentRating = table.Column<bool>(type: "boolean", nullable: false),
                    EnableModelFlagging = table.Column<bool>(type: "boolean", nullable: false),
                    RequireFlagReason = table.Column<bool>(type: "boolean", nullable: false),
                    EnableModelReporting = table.Column<bool>(type: "boolean", nullable: false),
                    RequireReportDetails = table.Column<bool>(type: "boolean", nullable: false),
                    EnableModelBlocking = table.Column<bool>(type: "boolean", nullable: false),
                    RequireBlockReason = table.Column<bool>(type: "boolean", nullable: false),
                    EnableModelWhitelisting = table.Column<bool>(type: "boolean", nullable: false),
                    EnableModelBlacklisting = table.Column<bool>(type: "boolean", nullable: false),
                    RequireModelApproval = table.Column<bool>(type: "boolean", nullable: false),
                    EnableModelRejection = table.Column<bool>(type: "boolean", nullable: false),
                    RequireRejectionReason = table.Column<bool>(type: "boolean", nullable: false),
                    EnableModelAppeals = table.Column<bool>(type: "boolean", nullable: false),
                    RequireAppealDetails = table.Column<bool>(type: "boolean", nullable: false),
                    EnableModelLocking = table.Column<bool>(type: "boolean", nullable: false),
                    RequireLockReason = table.Column<bool>(type: "boolean", nullable: false),
                    EnableModelUnlocking = table.Column<bool>(type: "boolean", nullable: false),
                    RequireUnlockApproval = table.Column<bool>(type: "boolean", nullable: false),
                    EnableModelDeletion = table.Column<bool>(type: "boolean", nullable: false),
                    RequireDeletionApproval = table.Column<bool>(type: "boolean", nullable: false),
                    RequireDeletionReason = table.Column<bool>(type: "boolean", nullable: false),
                    EnableModelRestoration = table.Column<bool>(type: "boolean", nullable: false),
                    RequireRestorationApproval = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedById = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedById = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModelSettings", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ModelSettings");
        }
    }
}
