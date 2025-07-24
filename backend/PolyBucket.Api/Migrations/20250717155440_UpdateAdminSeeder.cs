using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAdminSeeder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "BanExpiresAt",
                table: "Users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BanReason",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "BannedAt",
                table: "Users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "BannedById",
                table: "Users",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "BannedByUserId",
                table: "Users",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsBanned",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "FederatedInstances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    BaseUrl = table.Column<string>(type: "text", nullable: false),
                    PublicKey = table.Column<string>(type: "text", nullable: false),
                    SharedSecret = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsTrusted = table.Column<bool>(type: "boolean", nullable: false),
                    LastSyncAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastHeartbeatAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastError = table.Column<string>(type: "text", nullable: true),
                    SyncIntervalMinutes = table.Column<int>(type: "integer", nullable: false),
                    AdminContact = table.Column<string>(type: "text", nullable: true),
                    Version = table.Column<string>(type: "text", nullable: true),
                    MaxModelsToSync = table.Column<int>(type: "integer", nullable: false),
                    AllowedCategories = table.Column<string>(type: "text", nullable: false),
                    AllowedRoles = table.Column<string>(type: "text", nullable: false),
                    SyncPublicOnly = table.Column<bool>(type: "boolean", nullable: false),
                    SyncFeaturedOnly = table.Column<bool>(type: "boolean", nullable: false),
                    ModelsShared = table.Column<int>(type: "integer", nullable: false),
                    ModelsReceived = table.Column<int>(type: "integer", nullable: false),
                    TotalBytesTransferred = table.Column<long>(type: "bigint", nullable: false),
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
                name: "FederationHandshakes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FederatedInstanceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Direction = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Challenge = table.Column<string>(type: "text", nullable: true),
                    Response = table.Column<string>(type: "text", nullable: true),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true),
                    InitiatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ProtocolVersion = table.Column<string>(type: "text", nullable: false),
                    ClientVersion = table.Column<string>(type: "text", nullable: true),
                    ServerVersion = table.Column<string>(type: "text", nullable: true),
                    TempPublicKey = table.Column<string>(type: "text", nullable: true),
                    KeyExchangeData = table.Column<string>(type: "text", nullable: true),
                    SignatureData = table.Column<string>(type: "text", nullable: true),
                    RemoteIpAddress = table.Column<string>(type: "text", nullable: true),
                    UserAgent = table.Column<string>(type: "text", nullable: true),
                    AttemptNumber = table.Column<int>(type: "integer", nullable: false),
                    MaxAttempts = table.Column<int>(type: "integer", nullable: false),
                    RemoteInstanceName = table.Column<string>(type: "text", nullable: true),
                    RemoteInstanceVersion = table.Column<string>(type: "text", nullable: true),
                    RemoteAdminContact = table.Column<string>(type: "text", nullable: true),
                    RemoteCapabilities = table.Column<string>(type: "text", nullable: true),
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
                        name: "FK_FederationHandshakes_FederatedInstances_FederatedInstanceId",
                        column: x => x.FederatedInstanceId,
                        principalTable: "FederatedInstances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_BannedByUserId",
                table: "Users",
                column: "BannedByUserId");

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
                name: "IX_FederationHandshakes_FederatedInstanceId",
                table: "FederationHandshakes",
                column: "FederatedInstanceId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Users_BannedByUserId",
                table: "Users",
                column: "BannedByUserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Users_BannedByUserId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "FederatedModels");

            migrationBuilder.DropTable(
                name: "FederationAuditLogs");

            migrationBuilder.DropTable(
                name: "FederationHandshakes");

            migrationBuilder.DropTable(
                name: "FederationSettings");

            migrationBuilder.DropTable(
                name: "FederatedInstances");

            migrationBuilder.DropIndex(
                name: "IX_Users_BannedByUserId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "BanExpiresAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "BanReason",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "BannedAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "BannedById",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "BannedByUserId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsBanned",
                table: "Users");
        }
    }
}
