using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class AddFirstTimeSetupFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasCompletedFirstTimeSetup",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresPasswordChange",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

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
                    DefaultModelPrivacy = table.Column<string>(type: "text", nullable: false),
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SystemSetups");

            migrationBuilder.DropColumn(
                name: "HasCompletedFirstTimeSetup",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "RequiresPasswordChange",
                table: "Users");
        }
    }
}
