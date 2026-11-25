using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PolyBucket.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddModelFeatureDisabledModes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CollectionsDisabledMode",
                table: "ModelSettings",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CommentsDisabledMode",
                table: "ModelSettings",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CollectionsDisabledMode",
                table: "ModelSettings");

            migrationBuilder.DropColumn(
                name: "CommentsDisabledMode",
                table: "ModelSettings");
        }
    }
}
