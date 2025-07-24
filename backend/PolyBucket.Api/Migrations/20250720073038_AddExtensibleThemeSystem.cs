using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class AddExtensibleThemeSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ActiveThemeId",
                table: "SystemSetups",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ThemeConfiguration",
                table: "SystemSetups",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActiveThemeId",
                table: "SystemSetups");

            migrationBuilder.DropColumn(
                name: "ThemeConfiguration",
                table: "SystemSetups");
        }
    }
}
