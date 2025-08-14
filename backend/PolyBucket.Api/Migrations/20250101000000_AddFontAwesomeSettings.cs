using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PolyBucket.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddFontAwesomeSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FontAwesomeSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IsProEnabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ProLicenseKey = table.Column<string>(type: "text", nullable: true),
                    ProKitUrl = table.Column<string>(type: "text", nullable: true),
                    UseProIcons = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    FallbackToFree = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    LastLicenseCheck = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsLicenseValid = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    LicenseErrorMessage = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedById = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedById = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FontAwesomeSettings", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FontAwesomeSettings");
        }
    }
}
