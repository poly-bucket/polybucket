using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PolyBucket.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddIsStorageConfigured : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsStorageConfigured",
                table: "SystemSetups",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsStorageConfigured",
                table: "SystemSetups");
        }
    }
}
