using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCustomSettingsFromUserSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ModelVersion_Models_ModelId",
                table: "ModelVersion");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ModelVersion",
                table: "ModelVersion");

            migrationBuilder.DropIndex(
                name: "IX_ModelVersion_ModelId",
                table: "ModelVersion");

            migrationBuilder.DropColumn(
                name: "CustomSettings",
                table: "UserSettings");

            migrationBuilder.RenameTable(
                name: "ModelVersion",
                newName: "ModelVersions");

            migrationBuilder.RenameColumn(
                name: "Version",
                table: "ModelVersions",
                newName: "VersionNumber");

            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:PostgresExtension:hstore", ",,");

            migrationBuilder.AddColumn<Guid>(
                name: "ModelVersionId",
                table: "ModelFiles",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "ModelId",
                table: "ModelVersions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FileUrl",
                table: "ModelVersions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "ModelVersions",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ThumbnailUrl",
                table: "ModelVersions",
                type: "text",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ModelVersions",
                table: "ModelVersions",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_ModelFiles_ModelVersionId",
                table: "ModelFiles",
                column: "ModelVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_ModelVersions_ModelId_VersionNumber",
                table: "ModelVersions",
                columns: new[] { "ModelId", "VersionNumber" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ModelFiles_ModelVersions_ModelVersionId",
                table: "ModelFiles",
                column: "ModelVersionId",
                principalTable: "ModelVersions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ModelVersions_Models_ModelId",
                table: "ModelVersions",
                column: "ModelId",
                principalTable: "Models",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ModelFiles_ModelVersions_ModelVersionId",
                table: "ModelFiles");

            migrationBuilder.DropForeignKey(
                name: "FK_ModelVersions_Models_ModelId",
                table: "ModelVersions");

            migrationBuilder.DropIndex(
                name: "IX_ModelFiles_ModelVersionId",
                table: "ModelFiles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ModelVersions",
                table: "ModelVersions");

            migrationBuilder.DropIndex(
                name: "IX_ModelVersions_ModelId_VersionNumber",
                table: "ModelVersions");

            migrationBuilder.DropColumn(
                name: "ModelVersionId",
                table: "ModelFiles");

            migrationBuilder.DropColumn(
                name: "FileUrl",
                table: "ModelVersions");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "ModelVersions");

            migrationBuilder.DropColumn(
                name: "ThumbnailUrl",
                table: "ModelVersions");

            migrationBuilder.RenameTable(
                name: "ModelVersions",
                newName: "ModelVersion");

            migrationBuilder.RenameColumn(
                name: "VersionNumber",
                table: "ModelVersion",
                newName: "Version");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:hstore", ",,");

            migrationBuilder.AddColumn<Dictionary<string, string>>(
                name: "CustomSettings",
                table: "UserSettings",
                type: "hstore",
                nullable: false);

            migrationBuilder.AlterColumn<Guid>(
                name: "ModelId",
                table: "ModelVersion",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ModelVersion",
                table: "ModelVersion",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_ModelVersion_ModelId",
                table: "ModelVersion",
                column: "ModelId");

            migrationBuilder.AddForeignKey(
                name: "FK_ModelVersion_Models_ModelId",
                table: "ModelVersion",
                column: "ModelId",
                principalTable: "Models",
                principalColumn: "Id");
        }
    }
}
