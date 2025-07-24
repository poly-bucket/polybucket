using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class AddThemeSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AccentColor",
                table: "SystemSetups",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AccentDarkColor",
                table: "SystemSetups",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AccentLightColor",
                table: "SystemSetups",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BackgroundPrimaryColor",
                table: "SystemSetups",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BackgroundSecondaryColor",
                table: "SystemSetups",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BackgroundTertiaryColor",
                table: "SystemSetups",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsThemeCustomized",
                table: "SystemSetups",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PrimaryColor",
                table: "SystemSetups",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PrimaryDarkColor",
                table: "SystemSetups",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PrimaryLightColor",
                table: "SystemSetups",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SecondaryColor",
                table: "SystemSetups",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SecondaryDarkColor",
                table: "SystemSetups",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SecondaryLightColor",
                table: "SystemSetups",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccentColor",
                table: "SystemSetups");

            migrationBuilder.DropColumn(
                name: "AccentDarkColor",
                table: "SystemSetups");

            migrationBuilder.DropColumn(
                name: "AccentLightColor",
                table: "SystemSetups");

            migrationBuilder.DropColumn(
                name: "BackgroundPrimaryColor",
                table: "SystemSetups");

            migrationBuilder.DropColumn(
                name: "BackgroundSecondaryColor",
                table: "SystemSetups");

            migrationBuilder.DropColumn(
                name: "BackgroundTertiaryColor",
                table: "SystemSetups");

            migrationBuilder.DropColumn(
                name: "IsThemeCustomized",
                table: "SystemSetups");

            migrationBuilder.DropColumn(
                name: "PrimaryColor",
                table: "SystemSetups");

            migrationBuilder.DropColumn(
                name: "PrimaryDarkColor",
                table: "SystemSetups");

            migrationBuilder.DropColumn(
                name: "PrimaryLightColor",
                table: "SystemSetups");

            migrationBuilder.DropColumn(
                name: "SecondaryColor",
                table: "SystemSetups");

            migrationBuilder.DropColumn(
                name: "SecondaryDarkColor",
                table: "SystemSetups");

            migrationBuilder.DropColumn(
                name: "SecondaryLightColor",
                table: "SystemSetups");
        }
    }
}
