using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PolyBucket.Database.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUnusedUserSettingsColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AllowDirectMessages",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "ShowActivityStatus",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "ShowProfileInSearch",
                table: "UserSettings");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AllowDirectMessages",
                table: "UserSettings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ShowActivityStatus",
                table: "UserSettings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ShowProfileInSearch",
                table: "UserSettings",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
