using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PolyBucket.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddModerationColumnsToSystemSetup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Model_Collection_CollectionId",
                table: "Model");

            migrationBuilder.DropForeignKey(
                name: "FK_Model_Users_UserId",
                table: "Model");

            migrationBuilder.DropForeignKey(
                name: "FK_ModelComment_ModelComment_ParentCommentId",
                table: "ModelComment");

            migrationBuilder.DropForeignKey(
                name: "FK_ModelComment_Models_ModelId",
                table: "ModelComment");

            migrationBuilder.DropForeignKey(
                name: "FK_ModelComment_Users_UserId",
                table: "ModelComment");

            migrationBuilder.DropForeignKey(
                name: "FK_ModelFiles_Models_ModelId",
                table: "ModelFiles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ModelComment",
                table: "ModelComment");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Model",
                table: "Model");

            migrationBuilder.DropColumn(
                name: "IsPrimary",
                table: "ModelFiles");

            migrationBuilder.DropColumn(
                name: "IsScanned",
                table: "ModelFiles");

            migrationBuilder.RenameTable(
                name: "ModelComment",
                newName: "ModelsModelComments");

            migrationBuilder.RenameTable(
                name: "Model",
                newName: "ModelsDb");

            migrationBuilder.RenameColumn(
                name: "UploadedAt",
                table: "ModelFiles",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "StorageProvider",
                table: "ModelFiles",
                newName: "FileUrl");

            migrationBuilder.RenameColumn(
                name: "StoragePath",
                table: "ModelFiles",
                newName: "FileFormat");

            migrationBuilder.RenameColumn(
                name: "FileSize",
                table: "ModelFiles",
                newName: "FileSizeBytes");

            migrationBuilder.RenameColumn(
                name: "FileHash",
                table: "ModelFiles",
                newName: "ContentType");

            migrationBuilder.RenameIndex(
                name: "IX_ModelComment_UserId",
                table: "ModelsModelComments",
                newName: "IX_ModelsModelComments_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_ModelComment_ParentCommentId",
                table: "ModelsModelComments",
                newName: "IX_ModelsModelComments_ParentCommentId");

            migrationBuilder.RenameIndex(
                name: "IX_ModelComment_ModelId",
                table: "ModelsModelComments",
                newName: "IX_ModelsModelComments_ModelId");

            migrationBuilder.RenameIndex(
                name: "IX_Model_UserId",
                table: "ModelsDb",
                newName: "IX_ModelsDb_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Model_CollectionId",
                table: "ModelsDb",
                newName: "IX_ModelsDb_CollectionId");

            migrationBuilder.AddColumn<bool>(
                name: "IsModerationConfigured",
                table: "SystemSetups",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ModeratorRoles",
                table: "SystemSetups",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "RequireUploadModeration",
                table: "SystemSetups",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql("ALTER TABLE \"ModelFiles\" ALTER COLUMN \"FileType\" TYPE integer USING 0");

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "ModelsModelComments",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "ThumbnailUrl",
                table: "ModelsDb",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "ModelsDb",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "FileUrl",
                table: "ModelsDb",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "ModelsDb",
                type: "character varying(5000)",
                maxLength: 5000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<Guid>(
                name: "CollectionId1",
                table: "ModelsDb",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DownloadCount",
                table: "ModelsDb",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "FileFormat",
                table: "ModelsDb",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "FileSizeBytes",
                table: "ModelsDb",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<bool>(
                name: "IsFeatured",
                table: "ModelsDb",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "License",
                table: "ModelsDb",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "LikeCount",
                table: "ModelsDb",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "ModeratedById",
                table: "ModelsDb",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModerationDate",
                table: "ModelsDb",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModerationReason",
                table: "ModelsDb",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ModerationStatus",
                table: "ModelsDb",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "ParentVersionId",
                table: "ModelsDb",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserId1",
                table: "ModelsDb",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VersionLabel",
                table: "ModelsDb",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ViewCount",
                table: "ModelsDb",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ModelsModelComments",
                table: "ModelsModelComments",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ModelsDb",
                table: "ModelsDb",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Category",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Slug = table.Column<string>(type: "text", nullable: false),
                    IconUrl = table.Column<string>(type: "text", nullable: false),
                    ParentCategoryId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Category", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Category_Category_ParentCategoryId",
                        column: x => x.ParentCategoryId,
                        principalTable: "Category",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "EntityModelComments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ModelId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Content = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    ParentCommentId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntityModelComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EntityModelComments_EntityModelComments_ParentCommentId",
                        column: x => x.ParentCommentId,
                        principalTable: "EntityModelComments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EntityModelComments_ModelsDb_ModelId",
                        column: x => x.ModelId,
                        principalTable: "ModelsDb",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EntityModelComments_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ModelFile",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ModelId = table.Column<Guid>(type: "uuid", nullable: false),
                    FileName = table.Column<string>(type: "text", nullable: false),
                    FileType = table.Column<string>(type: "text", nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    StoragePath = table.Column<string>(type: "text", nullable: false),
                    StorageProvider = table.Column<string>(type: "text", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FileHash = table.Column<string>(type: "text", nullable: false),
                    IsScanned = table.Column<bool>(type: "boolean", nullable: false),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModelFile", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ModelFile_Models_ModelId",
                        column: x => x.ModelId,
                        principalTable: "Models",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tag",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Slug = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tag", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserModelInteraction",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ModelId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserModelInteraction", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserModelInteraction_ModelsDb_ModelId",
                        column: x => x.ModelId,
                        principalTable: "ModelsDb",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserModelInteraction_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ModelCategory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ModelId = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModelCategory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ModelCategory_Category_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Category",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ModelCategory_ModelsDb_ModelId",
                        column: x => x.ModelId,
                        principalTable: "ModelsDb",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ModelTag",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ModelId = table.Column<Guid>(type: "uuid", nullable: false),
                    TagId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModelTag", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ModelTag_ModelsDb_ModelId",
                        column: x => x.ModelId,
                        principalTable: "ModelsDb",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ModelTag_Tag_TagId",
                        column: x => x.TagId,
                        principalTable: "Tag",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ModelsDb_CollectionId1",
                table: "ModelsDb",
                column: "CollectionId1");

            migrationBuilder.CreateIndex(
                name: "IX_ModelsDb_ModeratedById",
                table: "ModelsDb",
                column: "ModeratedById");

            migrationBuilder.CreateIndex(
                name: "IX_ModelsDb_ParentVersionId",
                table: "ModelsDb",
                column: "ParentVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_ModelsDb_UserId1",
                table: "ModelsDb",
                column: "UserId1");

            migrationBuilder.CreateIndex(
                name: "IX_Category_ParentCategoryId",
                table: "Category",
                column: "ParentCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_EntityModelComments_ModelId",
                table: "EntityModelComments",
                column: "ModelId");

            migrationBuilder.CreateIndex(
                name: "IX_EntityModelComments_ParentCommentId",
                table: "EntityModelComments",
                column: "ParentCommentId");

            migrationBuilder.CreateIndex(
                name: "IX_EntityModelComments_UserId",
                table: "EntityModelComments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ModelCategory_CategoryId",
                table: "ModelCategory",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ModelCategory_ModelId",
                table: "ModelCategory",
                column: "ModelId");

            migrationBuilder.CreateIndex(
                name: "IX_ModelFile_ModelId",
                table: "ModelFile",
                column: "ModelId");

            migrationBuilder.CreateIndex(
                name: "IX_ModelTag_ModelId",
                table: "ModelTag",
                column: "ModelId");

            migrationBuilder.CreateIndex(
                name: "IX_ModelTag_TagId",
                table: "ModelTag",
                column: "TagId");

            migrationBuilder.CreateIndex(
                name: "IX_UserModelInteraction_ModelId",
                table: "UserModelInteraction",
                column: "ModelId");

            migrationBuilder.CreateIndex(
                name: "IX_UserModelInteraction_UserId",
                table: "UserModelInteraction",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ModelFiles_ModelsDb_ModelId",
                table: "ModelFiles",
                column: "ModelId",
                principalTable: "ModelsDb",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ModelsDb_Collection_CollectionId",
                table: "ModelsDb",
                column: "CollectionId",
                principalTable: "Collection",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ModelsDb_Collection_CollectionId1",
                table: "ModelsDb",
                column: "CollectionId1",
                principalTable: "Collection",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ModelsDb_ModelsDb_ParentVersionId",
                table: "ModelsDb",
                column: "ParentVersionId",
                principalTable: "ModelsDb",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ModelsDb_Users_ModeratedById",
                table: "ModelsDb",
                column: "ModeratedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ModelsDb_Users_UserId",
                table: "ModelsDb",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ModelsDb_Users_UserId1",
                table: "ModelsDb",
                column: "UserId1",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ModelsModelComments_ModelsModelComments_ParentCommentId",
                table: "ModelsModelComments",
                column: "ParentCommentId",
                principalTable: "ModelsModelComments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ModelsModelComments_Models_ModelId",
                table: "ModelsModelComments",
                column: "ModelId",
                principalTable: "Models",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ModelsModelComments_Users_UserId",
                table: "ModelsModelComments",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ModelFiles_ModelsDb_ModelId",
                table: "ModelFiles");

            migrationBuilder.DropForeignKey(
                name: "FK_ModelsDb_Collection_CollectionId",
                table: "ModelsDb");

            migrationBuilder.DropForeignKey(
                name: "FK_ModelsDb_Collection_CollectionId1",
                table: "ModelsDb");

            migrationBuilder.DropForeignKey(
                name: "FK_ModelsDb_ModelsDb_ParentVersionId",
                table: "ModelsDb");

            migrationBuilder.DropForeignKey(
                name: "FK_ModelsDb_Users_ModeratedById",
                table: "ModelsDb");

            migrationBuilder.DropForeignKey(
                name: "FK_ModelsDb_Users_UserId",
                table: "ModelsDb");

            migrationBuilder.DropForeignKey(
                name: "FK_ModelsDb_Users_UserId1",
                table: "ModelsDb");

            migrationBuilder.DropForeignKey(
                name: "FK_ModelsModelComments_ModelsModelComments_ParentCommentId",
                table: "ModelsModelComments");

            migrationBuilder.DropForeignKey(
                name: "FK_ModelsModelComments_Models_ModelId",
                table: "ModelsModelComments");

            migrationBuilder.DropForeignKey(
                name: "FK_ModelsModelComments_Users_UserId",
                table: "ModelsModelComments");

            migrationBuilder.DropTable(
                name: "EntityModelComments");

            migrationBuilder.DropTable(
                name: "ModelCategory");

            migrationBuilder.DropTable(
                name: "ModelFile");

            migrationBuilder.DropTable(
                name: "ModelTag");

            migrationBuilder.DropTable(
                name: "UserModelInteraction");

            migrationBuilder.DropTable(
                name: "Category");

            migrationBuilder.DropTable(
                name: "Tag");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ModelsModelComments",
                table: "ModelsModelComments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ModelsDb",
                table: "ModelsDb");

            migrationBuilder.DropIndex(
                name: "IX_ModelsDb_CollectionId1",
                table: "ModelsDb");

            migrationBuilder.DropIndex(
                name: "IX_ModelsDb_ModeratedById",
                table: "ModelsDb");

            migrationBuilder.DropIndex(
                name: "IX_ModelsDb_ParentVersionId",
                table: "ModelsDb");

            migrationBuilder.DropIndex(
                name: "IX_ModelsDb_UserId1",
                table: "ModelsDb");

            migrationBuilder.DropColumn(
                name: "IsModerationConfigured",
                table: "SystemSetups");

            migrationBuilder.DropColumn(
                name: "ModeratorRoles",
                table: "SystemSetups");

            migrationBuilder.DropColumn(
                name: "RequireUploadModeration",
                table: "SystemSetups");

            migrationBuilder.DropColumn(
                name: "CollectionId1",
                table: "ModelsDb");

            migrationBuilder.DropColumn(
                name: "DownloadCount",
                table: "ModelsDb");

            migrationBuilder.DropColumn(
                name: "FileFormat",
                table: "ModelsDb");

            migrationBuilder.DropColumn(
                name: "FileSizeBytes",
                table: "ModelsDb");

            migrationBuilder.DropColumn(
                name: "IsFeatured",
                table: "ModelsDb");

            migrationBuilder.DropColumn(
                name: "License",
                table: "ModelsDb");

            migrationBuilder.DropColumn(
                name: "LikeCount",
                table: "ModelsDb");

            migrationBuilder.DropColumn(
                name: "ModeratedById",
                table: "ModelsDb");

            migrationBuilder.DropColumn(
                name: "ModerationDate",
                table: "ModelsDb");

            migrationBuilder.DropColumn(
                name: "ModerationReason",
                table: "ModelsDb");

            migrationBuilder.DropColumn(
                name: "ModerationStatus",
                table: "ModelsDb");

            migrationBuilder.DropColumn(
                name: "ParentVersionId",
                table: "ModelsDb");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "ModelsDb");

            migrationBuilder.DropColumn(
                name: "VersionLabel",
                table: "ModelsDb");

            migrationBuilder.DropColumn(
                name: "ViewCount",
                table: "ModelsDb");

            migrationBuilder.RenameTable(
                name: "ModelsModelComments",
                newName: "ModelComment");

            migrationBuilder.RenameTable(
                name: "ModelsDb",
                newName: "Model");

            migrationBuilder.RenameColumn(
                name: "FileUrl",
                table: "ModelFiles",
                newName: "StorageProvider");

            migrationBuilder.RenameColumn(
                name: "FileSizeBytes",
                table: "ModelFiles",
                newName: "FileSize");

            migrationBuilder.RenameColumn(
                name: "FileFormat",
                table: "ModelFiles",
                newName: "StoragePath");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "ModelFiles",
                newName: "UploadedAt");

            migrationBuilder.RenameColumn(
                name: "ContentType",
                table: "ModelFiles",
                newName: "FileHash");

            migrationBuilder.RenameIndex(
                name: "IX_ModelsModelComments_UserId",
                table: "ModelComment",
                newName: "IX_ModelComment_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_ModelsModelComments_ParentCommentId",
                table: "ModelComment",
                newName: "IX_ModelComment_ParentCommentId");

            migrationBuilder.RenameIndex(
                name: "IX_ModelsModelComments_ModelId",
                table: "ModelComment",
                newName: "IX_ModelComment_ModelId");

            migrationBuilder.RenameIndex(
                name: "IX_ModelsDb_UserId",
                table: "Model",
                newName: "IX_Model_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_ModelsDb_CollectionId",
                table: "Model",
                newName: "IX_Model_CollectionId");

            migrationBuilder.AlterColumn<string>(
                name: "FileType",
                table: "ModelFiles",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<bool>(
                name: "IsPrimary",
                table: "ModelFiles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsScanned",
                table: "ModelFiles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "ModelComment",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000);

            migrationBuilder.AlterColumn<string>(
                name: "ThumbnailUrl",
                table: "Model",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Model",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "FileUrl",
                table: "Model",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Model",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(5000)",
                oldMaxLength: 5000);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ModelComment",
                table: "ModelComment",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Model",
                table: "Model",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Model_Collection_CollectionId",
                table: "Model",
                column: "CollectionId",
                principalTable: "Collection",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Model_Users_UserId",
                table: "Model",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ModelComment_ModelComment_ParentCommentId",
                table: "ModelComment",
                column: "ParentCommentId",
                principalTable: "ModelComment",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ModelComment_Models_ModelId",
                table: "ModelComment",
                column: "ModelId",
                principalTable: "Models",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ModelComment_Users_UserId",
                table: "ModelComment",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ModelFiles_Models_ModelId",
                table: "ModelFiles",
                column: "ModelId",
                principalTable: "Models",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
