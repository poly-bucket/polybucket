using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class initmigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Icon = table.Column<string>(type: "text", nullable: false),
                    Color = table.Column<string>(type: "text", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedById = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedById = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmailVerificationTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Token = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsUsed = table.Column<bool>(type: "boolean", nullable: false),
                    UsedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedByIp = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailVerificationTokens", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FederatedInstances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    BaseUrl = table.Column<string>(type: "text", nullable: false),
                    PublicKey = table.Column<string>(type: "text", nullable: true),
                    SharedSecret = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    LastSyncAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    OurToken = table.Column<string>(type: "text", nullable: true),
                    OurTokenExpiry = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TheirToken = table.Column<string>(type: "text", nullable: true),
                    TheirTokenExpiry = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TokenRenewalMode = table.Column<int>(type: "integer", nullable: false),
                    SyncMode = table.Column<int>(type: "integer", nullable: false),
                    SyncInterval = table.Column<int>(type: "integer", nullable: true),
                    AutoImportNewModels = table.Column<bool>(type: "boolean", nullable: false),
                    NextSyncAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedById = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedById = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FederatedInstances", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FederationSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IsFederationEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    InstanceName = table.Column<string>(type: "text", nullable: false),
                    InstanceDescription = table.Column<string>(type: "text", nullable: false),
                    AdminContact = table.Column<string>(type: "text", nullable: false),
                    BaseUrl = table.Column<string>(type: "text", nullable: false),
                    PrivateKey = table.Column<string>(type: "text", nullable: false),
                    PublicKey = table.Column<string>(type: "text", nullable: false),
                    KeyRotationIntervalDays = table.Column<int>(type: "integer", nullable: false),
                    LastKeyRotation = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RequireHttps = table.Column<bool>(type: "boolean", nullable: false),
                    AllowSelfSignedCertificates = table.Column<bool>(type: "boolean", nullable: false),
                    HandshakeTimeoutMinutes = table.Column<int>(type: "integer", nullable: false),
                    DefaultSyncIntervalMinutes = table.Column<int>(type: "integer", nullable: false),
                    MaxConcurrentSyncs = table.Column<int>(type: "integer", nullable: false),
                    MaxModelsPerInstance = table.Column<int>(type: "integer", nullable: false),
                    MaxModelFileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    ModelCacheRetentionDays = table.Column<int>(type: "integer", nullable: false),
                    AutoAcceptHandshakes = table.Column<bool>(type: "boolean", nullable: false),
                    SharePublicModelsOnly = table.Column<bool>(type: "boolean", nullable: false),
                    ShareFeaturedModelsOnly = table.Column<bool>(type: "boolean", nullable: false),
                    AllowedFileTypes = table.Column<string>(type: "text", nullable: false),
                    BlockedCategories = table.Column<string>(type: "text", nullable: false),
                    AllowNSFWContent = table.Column<bool>(type: "boolean", nullable: false),
                    RequireApprovalForNewInstances = table.Column<bool>(type: "boolean", nullable: false),
                    MaxRequestsPerMinute = table.Column<int>(type: "integer", nullable: false),
                    MaxDownloadsPerHour = table.Column<int>(type: "integer", nullable: false),
                    MaxBandwidthPerDay = table.Column<long>(type: "bigint", nullable: false),
                    HeartbeatIntervalMinutes = table.Column<int>(type: "integer", nullable: false),
                    HealthCheckTimeoutSeconds = table.Column<int>(type: "integer", nullable: false),
                    EnableMetrics = table.Column<bool>(type: "boolean", nullable: false),
                    EnableDetailedLogging = table.Column<bool>(type: "boolean", nullable: false),
                    FederationInviteUrl = table.Column<string>(type: "text", nullable: false),
                    InviteUrlGeneratedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    InviteUrlExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EnableDiscovery = table.Column<bool>(type: "boolean", nullable: false),
                    DiscoveryTags = table.Column<string>(type: "text", nullable: true),
                    EnableAutomaticBackup = table.Column<bool>(type: "boolean", nullable: false),
                    BackupRetentionDays = table.Column<int>(type: "integer", nullable: false),
                    LastBackupAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FederationProtocolVersion = table.Column<string>(type: "text", nullable: false),
                    SupportedVersions = table.Column<string>(type: "text", nullable: true),
                    TotalConnectedInstances = table.Column<int>(type: "integer", nullable: false),
                    TotalModelsShared = table.Column<int>(type: "integer", nullable: false),
                    TotalModelsReceived = table.Column<int>(type: "integer", nullable: false),
                    TotalBytesTransferred = table.Column<long>(type: "bigint", nullable: false),
                    LastStatsUpdate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FederationSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "filaments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    manufacturer = table.Column<string>(type: "varchar(255)", nullable: false),
                    type = table.Column<int>(type: "int", nullable: false),
                    color = table.Column<string>(type: "varchar(50)", nullable: false),
                    diameter = table.Column<string>(type: "varchar(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_filaments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FileTypeSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FileExtension = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Enabled = table.Column<bool>(type: "boolean", nullable: false),
                    MaxFileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    MaxPerUpload = table.Column<int>(type: "integer", nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    MimeType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    RequiresPreview = table.Column<bool>(type: "boolean", nullable: false),
                    IsCompressible = table.Column<bool>(type: "boolean", nullable: false),
                    Category = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedById = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedById = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileTypeSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FontAwesomeSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IsProEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    ProLicenseKey = table.Column<string>(type: "text", nullable: true),
                    ProKitUrl = table.Column<string>(type: "text", nullable: true),
                    UseProIcons = table.Column<bool>(type: "boolean", nullable: false),
                    FallbackToFree = table.Column<bool>(type: "boolean", nullable: false),
                    LastLicenseCheck = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsLicenseValid = table.Column<bool>(type: "boolean", nullable: false),
                    LicenseErrorMessage = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FontAwesomeSettings", x => x.Id);
                });

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

            migrationBuilder.CreateTable(
                name: "PasswordResetTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Token = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsUsed = table.Column<bool>(type: "boolean", nullable: false),
                    UsedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedByIp = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PasswordResetTokens", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Permissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Category = table.Column<string>(type: "text", nullable: false),
                    IsSystemPermission = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Printers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Manufacturer = table.Column<string>(type: "text", nullable: false),
                    Model = table.Column<string>(type: "text", nullable: false),
                    BuildVolumeX = table.Column<int>(type: "integer", nullable: false),
                    BuildVolumeY = table.Column<int>(type: "integer", nullable: false),
                    BuildVolumeZ = table.Column<int>(type: "integer", nullable: false),
                    MaxBedTemp = table.Column<int>(type: "integer", nullable: false),
                    MaxNozzleTemp = table.Column<int>(type: "integer", nullable: false),
                    HasHeatedBed = table.Column<bool>(type: "boolean", nullable: false),
                    HasHeatedChamber = table.Column<bool>(type: "boolean", nullable: false),
                    HasEnclosure = table.Column<bool>(type: "boolean", nullable: false),
                    DefaultNozzleDiameter = table.Column<decimal>(type: "numeric", nullable: false),
                    ExtruderCount = table.Column<int>(type: "integer", nullable: false),
                    ExtruderType = table.Column<int>(type: "integer", nullable: false),
                    Connectivity = table.Column<int[]>(type: "integer[]", nullable: false),
                    SupportedMaterials = table.Column<int[]>(type: "integer[]", nullable: false),
                    PriceUSD = table.Column<decimal>(type: "numeric", nullable: true),
                    ReleaseYear = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    HasAutoLeveling = table.Column<bool>(type: "boolean", nullable: false),
                    HasFilamentSensor = table.Column<bool>(type: "boolean", nullable: false),
                    HasPowerLossContinue = table.Column<bool>(type: "boolean", nullable: false),
                    MaxPrintSpeed = table.Column<decimal>(type: "numeric", nullable: false),
                    WebsiteUrl = table.Column<string>(type: "text", nullable: true),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedById = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedById = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Printers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Reports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    TargetId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReporterId = table.Column<Guid>(type: "uuid", nullable: false),
                    Reason = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    IsResolved = table.Column<bool>(type: "boolean", nullable: false),
                    Resolution = table.Column<string>(type: "text", nullable: false),
                    ResolvedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ResolvedById = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedById = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedById = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    IsSystemRole = table.Column<bool>(type: "boolean", nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    CanBeDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Color = table.Column<string>(type: "text", nullable: false),
                    ParentRoleId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Roles_Roles_ParentRoleId",
                        column: x => x.ParentRoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SystemSettings",
                columns: table => new
                {
                    Key = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemSettings", x => x.Key);
                });

            migrationBuilder.CreateTable(
                name: "SystemSetups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IsFirstTimeSetup = table.Column<bool>(type: "boolean", nullable: false),
                    IsAdminConfigured = table.Column<bool>(type: "boolean", nullable: false),
                    IsSiteConfigured = table.Column<bool>(type: "boolean", nullable: false),
                    IsEmailConfigured = table.Column<bool>(type: "boolean", nullable: false),
                    IsModerationConfigured = table.Column<bool>(type: "boolean", nullable: false),
                    SiteName = table.Column<string>(type: "text", nullable: false),
                    SiteDescription = table.Column<string>(type: "text", nullable: false),
                    ContactEmail = table.Column<string>(type: "text", nullable: false),
                    AllowPublicBrowsing = table.Column<bool>(type: "boolean", nullable: false),
                    RequireLoginForUpload = table.Column<bool>(type: "boolean", nullable: false),
                    AllowUserRegistration = table.Column<bool>(type: "boolean", nullable: false),
                    RequireEmailVerification = table.Column<bool>(type: "boolean", nullable: false),
                    MaxFileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    AllowedFileTypes = table.Column<string>(type: "text", nullable: false),
                    MaxFilesPerUpload = table.Column<int>(type: "integer", nullable: false),
                    EnableFileCompression = table.Column<bool>(type: "boolean", nullable: false),
                    AutoGeneratePreviews = table.Column<bool>(type: "boolean", nullable: false),
                    DefaultModelPrivacy = table.Column<int>(type: "integer", nullable: false),
                    AutoApproveModels = table.Column<bool>(type: "boolean", nullable: false),
                    RequireModeration = table.Column<bool>(type: "boolean", nullable: false),
                    DefaultUserRole = table.Column<string>(type: "text", nullable: false),
                    AllowCustomRoles = table.Column<bool>(type: "boolean", nullable: false),
                    MaxFailedLoginAttempts = table.Column<int>(type: "integer", nullable: false),
                    LockoutDurationMinutes = table.Column<int>(type: "integer", nullable: false),
                    RequireStrongPasswords = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordMinLength = table.Column<int>(type: "integer", nullable: false),
                    DefaultTheme = table.Column<string>(type: "text", nullable: false),
                    DefaultLanguage = table.Column<string>(type: "text", nullable: false),
                    ShowAdvancedOptions = table.Column<bool>(type: "boolean", nullable: false),
                    PrimaryColor = table.Column<string>(type: "text", nullable: false),
                    PrimaryLightColor = table.Column<string>(type: "text", nullable: false),
                    PrimaryDarkColor = table.Column<string>(type: "text", nullable: false),
                    SecondaryColor = table.Column<string>(type: "text", nullable: false),
                    SecondaryLightColor = table.Column<string>(type: "text", nullable: false),
                    SecondaryDarkColor = table.Column<string>(type: "text", nullable: false),
                    AccentColor = table.Column<string>(type: "text", nullable: false),
                    AccentLightColor = table.Column<string>(type: "text", nullable: false),
                    AccentDarkColor = table.Column<string>(type: "text", nullable: false),
                    BackgroundPrimaryColor = table.Column<string>(type: "text", nullable: false),
                    BackgroundSecondaryColor = table.Column<string>(type: "text", nullable: false),
                    BackgroundTertiaryColor = table.Column<string>(type: "text", nullable: false),
                    IsThemeCustomized = table.Column<bool>(type: "boolean", nullable: false),
                    ActiveThemeId = table.Column<string>(type: "text", nullable: true),
                    ThemeConfiguration = table.Column<string>(type: "text", nullable: true),
                    EnableFederation = table.Column<bool>(type: "boolean", nullable: false),
                    InstanceName = table.Column<string>(type: "text", nullable: false),
                    InstanceDescription = table.Column<string>(type: "text", nullable: false),
                    AdminContact = table.Column<string>(type: "text", nullable: false),
                    SetupCompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SetupCompletedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedById = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedById = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemSetups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Color = table.Column<string>(type: "text", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedById = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedById = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Themes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Themes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FederationHandshakes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InitiatorInstanceId = table.Column<Guid>(type: "uuid", nullable: true),
                    ResponderInstanceId = table.Column<Guid>(type: "uuid", nullable: true),
                    InitiatorUrl = table.Column<string>(type: "text", nullable: false),
                    ResponderUrl = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    HandshakeToken = table.Column<string>(type: "text", nullable: true),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedById = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedById = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FederationHandshakes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FederationHandshakes_FederatedInstances_InitiatorInstanceId",
                        column: x => x.InitiatorInstanceId,
                        principalTable: "FederatedInstances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_FederationHandshakes_FederatedInstances_ResponderInstanceId",
                        column: x => x.ResponderInstanceId,
                        principalTable: "FederatedInstances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    Username = table.Column<string>(type: "text", nullable: false),
                    FirstName = table.Column<string>(type: "text", nullable: true),
                    LastName = table.Column<string>(type: "text", nullable: true),
                    Bio = table.Column<string>(type: "text", nullable: true),
                    Salt = table.Column<string>(type: "text", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: true),
                    Country = table.Column<string>(type: "text", nullable: true),
                    IsBanned = table.Column<bool>(type: "boolean", nullable: false),
                    BannedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    BannedById = table.Column<Guid>(type: "uuid", nullable: true),
                    BannedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    BanReason = table.Column<string>(type: "text", nullable: true),
                    BanExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    HasCompletedFirstTimeSetup = table.Column<bool>(type: "boolean", nullable: false),
                    RequiresPasswordChange = table.Column<bool>(type: "boolean", nullable: false),
                    Avatar = table.Column<string>(type: "text", nullable: true),
                    ProfilePictureUrl = table.Column<string>(type: "text", nullable: true),
                    LastLoginAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    WebsiteUrl = table.Column<string>(type: "text", nullable: true),
                    TwitterUrl = table.Column<string>(type: "text", nullable: true),
                    InstagramUrl = table.Column<string>(type: "text", nullable: true),
                    YouTubeUrl = table.Column<string>(type: "text", nullable: true),
                    IsProfilePublic = table.Column<bool>(type: "boolean", nullable: false),
                    ShowEmail = table.Column<bool>(type: "boolean", nullable: false),
                    ShowLastLogin = table.Column<bool>(type: "boolean", nullable: false),
                    ShowStatistics = table.Column<bool>(type: "boolean", nullable: false),
                    RemoteInstanceId = table.Column<string>(type: "text", nullable: true),
                    RemoteUserId = table.Column<string>(type: "text", nullable: true),
                    IsFederated = table.Column<bool>(type: "boolean", nullable: false),
                    CanLogin = table.Column<bool>(type: "boolean", nullable: false),
                    LastFederationSync = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedById = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedById = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Users_Users_BannedByUserId",
                        column: x => x.BannedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ThemeColors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ThemeId = table.Column<int>(type: "integer", nullable: false),
                    Primary = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    PrimaryLight = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    PrimaryDark = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    Secondary = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    SecondaryLight = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    SecondaryDark = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    Accent = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    AccentLight = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    AccentDark = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    BackgroundPrimary = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    BackgroundSecondary = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    BackgroundTertiary = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThemeColors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ThemeColors_Themes_ThemeId",
                        column: x => x.ThemeId,
                        principalTable: "Themes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Collections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Visibility = table.Column<int>(type: "integer", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    Avatar = table.Column<string>(type: "text", nullable: true),
                    Favorite = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    OwnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedById = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedById = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Collections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Collections_Users_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExternalAuthProviders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Provider = table.Column<string>(type: "text", nullable: false),
                    ExternalId = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Picture = table.Column<string>(type: "text", nullable: true),
                    LastLoginAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExternalAuthProviders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExternalAuthProviders_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FederationAuditLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FederatedInstanceId = table.Column<Guid>(type: "uuid", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Action = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Details = table.Column<string>(type: "text", nullable: true),
                    RequestId = table.Column<string>(type: "text", nullable: true),
                    HttpMethod = table.Column<string>(type: "text", nullable: true),
                    EndpointPath = table.Column<string>(type: "text", nullable: true),
                    HttpStatusCode = table.Column<int>(type: "integer", nullable: true),
                    RequestIpAddress = table.Column<string>(type: "text", nullable: true),
                    UserAgent = table.Column<string>(type: "text", nullable: true),
                    AffectedResourceType = table.Column<string>(type: "text", nullable: true),
                    AffectedResourceId = table.Column<string>(type: "text", nullable: true),
                    PreviousValues = table.Column<string>(type: "text", nullable: true),
                    NewValues = table.Column<string>(type: "text", nullable: true),
                    ResponseTimeMs = table.Column<long>(type: "bigint", nullable: true),
                    DataTransferredBytes = table.Column<long>(type: "bigint", nullable: true),
                    IsSuccessful = table.Column<bool>(type: "boolean", nullable: false),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true),
                    SecurityFlags = table.Column<string>(type: "text", nullable: true),
                    RiskLevel = table.Column<string>(type: "text", nullable: true),
                    Metadata = table.Column<string>(type: "text", nullable: true),
                    IsInternal = table.Column<bool>(type: "boolean", nullable: false),
                    CorrelationId = table.Column<string>(type: "text", nullable: true),
                    EventTimestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedById = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedById = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FederationAuditLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FederationAuditLogs_FederatedInstances_FederatedInstanceId",
                        column: x => x.FederatedInstanceId,
                        principalTable: "FederatedInstances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_FederationAuditLogs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Models",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    ThumbnailUrl = table.Column<string>(type: "text", nullable: true),
                    FileUrl = table.Column<string>(type: "text", nullable: true),
                    Downloads = table.Column<int>(type: "integer", nullable: false),
                    Likes = table.Column<int>(type: "integer", nullable: false),
                    License = table.Column<int>(type: "integer", nullable: true),
                    Privacy = table.Column<int>(type: "integer", nullable: false),
                    AIGenerated = table.Column<bool>(type: "boolean", nullable: false),
                    WIP = table.Column<bool>(type: "boolean", nullable: false),
                    NSFW = table.Column<bool>(type: "boolean", nullable: false),
                    IsRemix = table.Column<bool>(type: "boolean", nullable: false),
                    RemixUrl = table.Column<string>(type: "text", nullable: true),
                    IsPublic = table.Column<bool>(type: "boolean", nullable: false),
                    IsFeatured = table.Column<bool>(type: "boolean", nullable: false),
                    AuthorId = table.Column<Guid>(type: "uuid", nullable: false),
                    RemoteInstanceId = table.Column<string>(type: "text", nullable: true),
                    RemoteModelId = table.Column<string>(type: "text", nullable: true),
                    RemoteAuthorId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsFederated = table.Column<bool>(type: "boolean", nullable: false),
                    LastFederationSync = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedById = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedById = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Models", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Models_Users_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ModerationAuditLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ModelId = table.Column<Guid>(type: "uuid", nullable: false),
                    PerformedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Action = table.Column<int>(type: "integer", nullable: false),
                    PreviousValues = table.Column<string>(type: "text", nullable: true),
                    NewValues = table.Column<string>(type: "text", nullable: true),
                    ModerationNotes = table.Column<string>(type: "text", nullable: true),
                    PerformedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IPAddress = table.Column<string>(type: "text", nullable: true),
                    UserAgent = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModerationAuditLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ModerationAuditLogs_Users_PerformedByUserId",
                        column: x => x.PerformedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Token = table.Column<string>(type: "text", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedByIp = table.Column<string>(type: "text", nullable: false),
                    RevokedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RevokedByIp = table.Column<string>(type: "text", nullable: true),
                    ReplacedByToken = table.Column<string>(type: "text", nullable: true),
                    ReasonRevoked = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RolePermissions",
                columns: table => new
                {
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    PermissionId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsGranted = table.Column<bool>(type: "boolean", nullable: false),
                    GrantedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermissions", x => new { x.RoleId, x.PermissionId });
                    table.ForeignKey(
                        name: "FK_RolePermissions_Permissions_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "Permissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RolePermissions_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RolePermissions_Users_GrantedByUserId",
                        column: x => x.GrantedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TwoFactorAuths",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    SecretKey = table.Column<string>(type: "text", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    EnabledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastUsedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RecoveryEmail = table.Column<string>(type: "text", nullable: true),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedById = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedById = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TwoFactorAuths", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TwoFactorAuths_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserLogins",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    Successful = table.Column<bool>(type: "boolean", nullable: true),
                    IpAddress = table.Column<string>(type: "text", nullable: true),
                    UserAgent = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLogins", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserLogins_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UserPermissions",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    PermissionId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsGranted = table.Column<bool>(type: "boolean", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Reason = table.Column<string>(type: "text", nullable: true),
                    GrantedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedById = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedById = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPermissions", x => new { x.UserId, x.PermissionId });
                    table.ForeignKey(
                        name: "FK_UserPermissions_Permissions_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "Permissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserPermissions_Users_GrantedByUserId",
                        column: x => x.GrantedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UserPermissions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Language = table.Column<string>(type: "text", nullable: false),
                    Theme = table.Column<string>(type: "text", nullable: false),
                    EmailNotifications = table.Column<bool>(type: "boolean", nullable: false),
                    DefaultPrinterId = table.Column<Guid>(type: "uuid", nullable: true),
                    MeasurementSystem = table.Column<string>(type: "text", nullable: false),
                    TimeZone = table.Column<string>(type: "text", nullable: false),
                    AutoRotateModels = table.Column<bool>(type: "boolean", nullable: false),
                    DashboardViewType = table.Column<string>(type: "text", nullable: false),
                    CardSize = table.Column<string>(type: "text", nullable: false),
                    CardSpacing = table.Column<string>(type: "text", nullable: false),
                    GridColumns = table.Column<int>(type: "integer", nullable: false),
                    ShowProfileInSearch = table.Column<bool>(type: "boolean", nullable: false),
                    AllowDirectMessages = table.Column<bool>(type: "boolean", nullable: false),
                    ShowActivityStatus = table.Column<bool>(type: "boolean", nullable: false),
                    NotifyOnMentions = table.Column<bool>(type: "boolean", nullable: false),
                    NotifyOnFollows = table.Column<bool>(type: "boolean", nullable: false),
                    NotifyOnLikes = table.Column<bool>(type: "boolean", nullable: false),
                    NotifyOnComments = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserSettings_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CategoryModel",
                columns: table => new
                {
                    CategoriesId = table.Column<Guid>(type: "uuid", nullable: false),
                    ModelsId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoryModel", x => new { x.CategoriesId, x.ModelsId });
                    table.ForeignKey(
                        name: "FK_CategoryModel_Categories_CategoriesId",
                        column: x => x.CategoriesId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CategoryModel_Models_ModelsId",
                        column: x => x.ModelsId,
                        principalTable: "Models",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CollectionModels",
                columns: table => new
                {
                    CollectionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ModelId = table.Column<Guid>(type: "uuid", nullable: false),
                    AddedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CollectionModels", x => new { x.CollectionId, x.ModelId });
                    table.ForeignKey(
                        name: "FK_CollectionModels_Collections_CollectionId",
                        column: x => x.CollectionId,
                        principalTable: "Collections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CollectionModels_Models_ModelId",
                        column: x => x.ModelId,
                        principalTable: "Models",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Comments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    Likes = table.Column<int>(type: "integer", nullable: false),
                    Dislikes = table.Column<int>(type: "integer", nullable: false),
                    ModelId = table.Column<Guid>(type: "uuid", nullable: false),
                    AuthorId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedById = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedById = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Comments_Models_ModelId",
                        column: x => x.ModelId,
                        principalTable: "Models",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Comments_Users_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FederatedModels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LocalModelId = table.Column<Guid>(type: "uuid", nullable: true),
                    RemoteModelId = table.Column<Guid>(type: "uuid", nullable: true),
                    RemoteModelName = table.Column<string>(type: "text", nullable: false),
                    RemoteModelDescription = table.Column<string>(type: "text", nullable: false),
                    RemoteAuthorName = table.Column<string>(type: "text", nullable: true),
                    RemoteCreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RemoteUpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RemoteThumbnailUrl = table.Column<string>(type: "text", nullable: true),
                    RemoteDownloads = table.Column<int>(type: "integer", nullable: false),
                    RemoteLikes = table.Column<int>(type: "integer", nullable: false),
                    RemoteCategories = table.Column<string>(type: "text", nullable: false),
                    RemoteTags = table.Column<string>(type: "text", nullable: false),
                    RemoteLicense = table.Column<string>(type: "text", nullable: true),
                    RemoteIsNSFW = table.Column<bool>(type: "boolean", nullable: false),
                    RemoteIsAIGenerated = table.Column<bool>(type: "boolean", nullable: false),
                    RemoteFileSize = table.Column<long>(type: "bigint", nullable: false),
                    RemoteFileFormats = table.Column<string>(type: "text", nullable: false),
                    FederatedInstanceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Direction = table.Column<int>(type: "integer", nullable: false),
                    SyncStatus = table.Column<int>(type: "integer", nullable: false),
                    LastSyncAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SyncError = table.Column<string>(type: "text", nullable: true),
                    MetadataHash = table.Column<string>(type: "text", nullable: false),
                    IsVisible = table.Column<bool>(type: "boolean", nullable: false),
                    SyncPriority = table.Column<int>(type: "integer", nullable: false),
                    HasLocalCache = table.Column<bool>(type: "boolean", nullable: false),
                    LocalCachePath = table.Column<string>(type: "text", nullable: true),
                    CacheExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CachedFileSize = table.Column<long>(type: "bigint", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedById = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedById = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FederatedModels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FederatedModels_FederatedInstances_FederatedInstanceId",
                        column: x => x.FederatedInstanceId,
                        principalTable: "FederatedInstances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FederatedModels_Models_LocalModelId",
                        column: x => x.LocalModelId,
                        principalTable: "Models",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Likes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ModelId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedById = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedById = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Likes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Likes_Models_ModelId",
                        column: x => x.ModelId,
                        principalTable: "Models",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Likes_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ModelPreviews",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ModelId = table.Column<Guid>(type: "uuid", nullable: false),
                    Size = table.Column<string>(type: "text", nullable: false),
                    PreviewUrl = table.Column<string>(type: "text", nullable: false),
                    StorageKey = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true),
                    GeneratedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Width = table.Column<int>(type: "integer", nullable: false),
                    Height = table.Column<int>(type: "integer", nullable: false),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedById = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedById = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModelPreviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ModelPreviews_Models_ModelId",
                        column: x => x.ModelId,
                        principalTable: "Models",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ModelTag",
                columns: table => new
                {
                    ModelsId = table.Column<Guid>(type: "uuid", nullable: false),
                    TagsId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModelTag", x => new { x.ModelsId, x.TagsId });
                    table.ForeignKey(
                        name: "FK_ModelTag_Models_ModelsId",
                        column: x => x.ModelsId,
                        principalTable: "Models",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ModelTag_Tags_TagsId",
                        column: x => x.TagsId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ModelVersions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: false),
                    FileUrl = table.Column<string>(type: "text", nullable: true),
                    ThumbnailUrl = table.Column<string>(type: "text", nullable: true),
                    VersionNumber = table.Column<int>(type: "integer", nullable: false),
                    ModelId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedById = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedById = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModelVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ModelVersions_Models_ModelId",
                        column: x => x.ModelId,
                        principalTable: "Models",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BackupCodes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TwoFactorAuthId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    IsUsed = table.Column<bool>(type: "boolean", nullable: false),
                    UsedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedById = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedById = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BackupCodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BackupCodes_TwoFactorAuths_TwoFactorAuthId",
                        column: x => x.TwoFactorAuthId,
                        principalTable: "TwoFactorAuths",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ModelFiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ModelId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Path = table.Column<string>(type: "text", nullable: false),
                    Size = table.Column<long>(type: "bigint", nullable: false),
                    MimeType = table.Column<string>(type: "text", nullable: false),
                    ModelVersionId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedById = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedById = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModelFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ModelFiles_ModelVersions_ModelVersionId",
                        column: x => x.ModelVersionId,
                        principalTable: "ModelVersions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ModelFiles_Models_ModelId",
                        column: x => x.ModelId,
                        principalTable: "Models",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BackupCodes_TwoFactorAuthId",
                table: "BackupCodes",
                column: "TwoFactorAuthId");

            migrationBuilder.CreateIndex(
                name: "IX_CategoryModel_ModelsId",
                table: "CategoryModel",
                column: "ModelsId");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionModels_ModelId",
                table: "CollectionModels",
                column: "ModelId");

            migrationBuilder.CreateIndex(
                name: "IX_Collections_OwnerId_Favorite_DisplayOrder",
                table: "Collections",
                columns: new[] { "OwnerId", "Favorite", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_Comments_AuthorId",
                table: "Comments",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_ModelId",
                table: "Comments",
                column: "ModelId");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalAuthProviders_UserId",
                table: "ExternalAuthProviders",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_FederatedModels_FederatedInstanceId",
                table: "FederatedModels",
                column: "FederatedInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_FederatedModels_LocalModelId",
                table: "FederatedModels",
                column: "LocalModelId");

            migrationBuilder.CreateIndex(
                name: "IX_FederationAuditLogs_FederatedInstanceId",
                table: "FederationAuditLogs",
                column: "FederatedInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_FederationAuditLogs_UserId",
                table: "FederationAuditLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_FederationHandshakes_InitiatorInstanceId",
                table: "FederationHandshakes",
                column: "InitiatorInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_FederationHandshakes_ResponderInstanceId",
                table: "FederationHandshakes",
                column: "ResponderInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_Likes_ModelId",
                table: "Likes",
                column: "ModelId");

            migrationBuilder.CreateIndex(
                name: "IX_Likes_UserId",
                table: "Likes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ModelFiles_ModelId",
                table: "ModelFiles",
                column: "ModelId");

            migrationBuilder.CreateIndex(
                name: "IX_ModelFiles_ModelVersionId",
                table: "ModelFiles",
                column: "ModelVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_ModelPreviews_ModelId_Size",
                table: "ModelPreviews",
                columns: new[] { "ModelId", "Size" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Models_AuthorId",
                table: "Models",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_ModelTag_TagsId",
                table: "ModelTag",
                column: "TagsId");

            migrationBuilder.CreateIndex(
                name: "IX_ModelVersions_ModelId_VersionNumber",
                table: "ModelVersions",
                columns: new[] { "ModelId", "VersionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ModerationAuditLogs_PerformedByUserId",
                table: "ModerationAuditLogs",
                column: "PerformedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId",
                table: "RefreshTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_GrantedByUserId",
                table: "RolePermissions",
                column: "GrantedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_PermissionId",
                table: "RolePermissions",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_ParentRoleId",
                table: "Roles",
                column: "ParentRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_ThemeColors_ThemeId",
                table: "ThemeColors",
                column: "ThemeId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Themes_Name",
                table: "Themes",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TwoFactorAuths_UserId",
                table: "TwoFactorAuths",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserLogins_UserId",
                table: "UserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPermissions_GrantedByUserId",
                table: "UserPermissions",
                column: "GrantedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPermissions_PermissionId",
                table: "UserPermissions",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_BannedByUserId",
                table: "Users",
                column: "BannedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_RoleId",
                table: "Users",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserSettings_UserId",
                table: "UserSettings",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BackupCodes");

            migrationBuilder.DropTable(
                name: "CategoryModel");

            migrationBuilder.DropTable(
                name: "CollectionModels");

            migrationBuilder.DropTable(
                name: "Comments");

            migrationBuilder.DropTable(
                name: "EmailVerificationTokens");

            migrationBuilder.DropTable(
                name: "ExternalAuthProviders");

            migrationBuilder.DropTable(
                name: "FederatedModels");

            migrationBuilder.DropTable(
                name: "FederationAuditLogs");

            migrationBuilder.DropTable(
                name: "FederationHandshakes");

            migrationBuilder.DropTable(
                name: "FederationSettings");

            migrationBuilder.DropTable(
                name: "filaments");

            migrationBuilder.DropTable(
                name: "FileTypeSettings");

            migrationBuilder.DropTable(
                name: "FontAwesomeSettings");

            migrationBuilder.DropTable(
                name: "Likes");

            migrationBuilder.DropTable(
                name: "ModelFiles");

            migrationBuilder.DropTable(
                name: "ModelPreviews");

            migrationBuilder.DropTable(
                name: "ModelSettings");

            migrationBuilder.DropTable(
                name: "ModelTag");

            migrationBuilder.DropTable(
                name: "ModerationAuditLogs");

            migrationBuilder.DropTable(
                name: "PasswordResetTokens");

            migrationBuilder.DropTable(
                name: "Printers");

            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropTable(
                name: "Reports");

            migrationBuilder.DropTable(
                name: "RolePermissions");

            migrationBuilder.DropTable(
                name: "SystemSettings");

            migrationBuilder.DropTable(
                name: "SystemSetups");

            migrationBuilder.DropTable(
                name: "ThemeColors");

            migrationBuilder.DropTable(
                name: "UserLogins");

            migrationBuilder.DropTable(
                name: "UserPermissions");

            migrationBuilder.DropTable(
                name: "UserSettings");

            migrationBuilder.DropTable(
                name: "TwoFactorAuths");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "Collections");

            migrationBuilder.DropTable(
                name: "FederatedInstances");

            migrationBuilder.DropTable(
                name: "ModelVersions");

            migrationBuilder.DropTable(
                name: "Tags");

            migrationBuilder.DropTable(
                name: "Themes");

            migrationBuilder.DropTable(
                name: "Permissions");

            migrationBuilder.DropTable(
                name: "Models");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Roles");
        }
    }
}
