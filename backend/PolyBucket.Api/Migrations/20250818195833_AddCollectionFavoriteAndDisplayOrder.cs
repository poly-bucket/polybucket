using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class AddCollectionFavoriteAndDisplayOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Collections_OwnerId",
                table: "Collections");

            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                table: "Collections",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "Favorite",
                table: "Collections",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Collections_OwnerId_Favorite_DisplayOrder",
                table: "Collections",
                columns: new[] { "OwnerId", "Favorite", "DisplayOrder" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Collections_OwnerId_Favorite_DisplayOrder",
                table: "Collections");

            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                table: "Collections");

            migrationBuilder.DropColumn(
                name: "Favorite",
                table: "Collections");

            migrationBuilder.CreateIndex(
                name: "IX_Collections_OwnerId",
                table: "Collections",
                column: "OwnerId");
        }
    }
}
