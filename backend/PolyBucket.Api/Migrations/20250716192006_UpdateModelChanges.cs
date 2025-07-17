using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class UpdateModelChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ModelFile_Models_ModelId",
                table: "ModelFile");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ModelFile",
                table: "ModelFile");

            migrationBuilder.RenameTable(
                name: "ModelFile",
                newName: "ModelFiles");

            migrationBuilder.RenameIndex(
                name: "IX_ModelFile_ModelId",
                table: "ModelFiles",
                newName: "IX_ModelFiles_ModelId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ModelFiles",
                table: "ModelFiles",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ModelFiles_Models_ModelId",
                table: "ModelFiles",
                column: "ModelId",
                principalTable: "Models",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ModelFiles_Models_ModelId",
                table: "ModelFiles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ModelFiles",
                table: "ModelFiles");

            migrationBuilder.RenameTable(
                name: "ModelFiles",
                newName: "ModelFile");

            migrationBuilder.RenameIndex(
                name: "IX_ModelFiles_ModelId",
                table: "ModelFile",
                newName: "IX_ModelFile_ModelId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ModelFile",
                table: "ModelFile",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ModelFile_Models_ModelId",
                table: "ModelFile",
                column: "ModelId",
                principalTable: "Models",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
