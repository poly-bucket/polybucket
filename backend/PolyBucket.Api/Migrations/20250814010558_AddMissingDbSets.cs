using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingDbSets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Like_Models_ModelId",
                table: "Like");

            migrationBuilder.DropForeignKey(
                name: "FK_Like_Users_UserId",
                table: "Like");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Like",
                table: "Like");

            migrationBuilder.RenameTable(
                name: "Like",
                newName: "Likes");

            migrationBuilder.RenameIndex(
                name: "IX_Like_UserId",
                table: "Likes",
                newName: "IX_Likes_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Like_ModelId",
                table: "Likes",
                newName: "IX_Likes_ModelId");

            migrationBuilder.AddColumn<bool>(
                name: "AllowDirectMessages",
                table: "UserSettings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "NotifyOnComments",
                table: "UserSettings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "NotifyOnFollows",
                table: "UserSettings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "NotifyOnLikes",
                table: "UserSettings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "NotifyOnMentions",
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

            migrationBuilder.AddColumn<string>(
                name: "Bio",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InstagramUrl",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsProfilePublic",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastLoginAt",
                table: "Users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProfilePictureUrl",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ShowEmail",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ShowLastLogin",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ShowStatistics",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "TwitterUrl",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WebsiteUrl",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "YouTubeUrl",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Likes",
                table: "Likes",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Likes_Models_ModelId",
                table: "Likes",
                column: "ModelId",
                principalTable: "Models",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Likes_Users_UserId",
                table: "Likes",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Likes_Models_ModelId",
                table: "Likes");

            migrationBuilder.DropForeignKey(
                name: "FK_Likes_Users_UserId",
                table: "Likes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Likes",
                table: "Likes");

            migrationBuilder.DropColumn(
                name: "AllowDirectMessages",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "NotifyOnComments",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "NotifyOnFollows",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "NotifyOnLikes",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "NotifyOnMentions",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "ShowActivityStatus",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "ShowProfileInSearch",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "Bio",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "InstagramUrl",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsProfilePublic",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastLoginAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ProfilePictureUrl",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ShowEmail",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ShowLastLogin",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ShowStatistics",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TwitterUrl",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "WebsiteUrl",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "YouTubeUrl",
                table: "Users");

            migrationBuilder.RenameTable(
                name: "Likes",
                newName: "Like");

            migrationBuilder.RenameIndex(
                name: "IX_Likes_UserId",
                table: "Like",
                newName: "IX_Like_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Likes_ModelId",
                table: "Like",
                newName: "IX_Like_ModelId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Like",
                table: "Like",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Like_Models_ModelId",
                table: "Like",
                column: "ModelId",
                principalTable: "Models",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Like_Users_UserId",
                table: "Like",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
