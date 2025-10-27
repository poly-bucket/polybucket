using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class AddFederationSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdminContact",
                table: "FederatedInstances");

            migrationBuilder.DropColumn(
                name: "AllowedCategories",
                table: "FederatedInstances");

            migrationBuilder.DropColumn(
                name: "AllowedRoles",
                table: "FederatedInstances");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "FederatedInstances");

            migrationBuilder.DropColumn(
                name: "IsTrusted",
                table: "FederatedInstances");

            migrationBuilder.DropColumn(
                name: "LastError",
                table: "FederatedInstances");

            migrationBuilder.DropColumn(
                name: "LastHeartbeatAt",
                table: "FederatedInstances");

            migrationBuilder.DropColumn(
                name: "MaxModelsToSync",
                table: "FederatedInstances");

            migrationBuilder.DropColumn(
                name: "ModelsReceived",
                table: "FederatedInstances");

            migrationBuilder.DropColumn(
                name: "ModelsShared",
                table: "FederatedInstances");

            migrationBuilder.DropColumn(
                name: "SyncFeaturedOnly",
                table: "FederatedInstances");

            migrationBuilder.DropColumn(
                name: "SyncIntervalMinutes",
                table: "FederatedInstances");

            migrationBuilder.DropColumn(
                name: "TotalBytesTransferred",
                table: "FederatedInstances");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "FederatedInstances");

            migrationBuilder.RenameColumn(
                name: "SyncPublicOnly",
                table: "FederatedInstances",
                newName: "IsEnabled");

            migrationBuilder.AddColumn<bool>(
                name: "CanLogin",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsFederated",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastFederationSync",
                table: "Users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RemoteInstanceId",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RemoteUserId",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFederated",
                table: "Models",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastFederationSync",
                table: "Models",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RemoteAuthorId",
                table: "Models",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RemoteInstanceId",
                table: "Models",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RemoteModelId",
                table: "Models",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SharedSecret",
                table: "FederatedInstances",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "PublicKey",
                table: "FederatedInstances",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "FederatedInstances",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CanLogin",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsFederated",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastFederationSync",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "RemoteInstanceId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "RemoteUserId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsFederated",
                table: "Models");

            migrationBuilder.DropColumn(
                name: "LastFederationSync",
                table: "Models");

            migrationBuilder.DropColumn(
                name: "RemoteAuthorId",
                table: "Models");

            migrationBuilder.DropColumn(
                name: "RemoteInstanceId",
                table: "Models");

            migrationBuilder.DropColumn(
                name: "RemoteModelId",
                table: "Models");

            migrationBuilder.RenameColumn(
                name: "IsEnabled",
                table: "FederatedInstances",
                newName: "SyncPublicOnly");

            migrationBuilder.AlterColumn<string>(
                name: "SharedSecret",
                table: "FederatedInstances",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PublicKey",
                table: "FederatedInstances",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "FederatedInstances",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AdminContact",
                table: "FederatedInstances",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AllowedCategories",
                table: "FederatedInstances",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AllowedRoles",
                table: "FederatedInstances",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "FederatedInstances",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsTrusted",
                table: "FederatedInstances",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LastError",
                table: "FederatedInstances",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastHeartbeatAt",
                table: "FederatedInstances",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxModelsToSync",
                table: "FederatedInstances",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ModelsReceived",
                table: "FederatedInstances",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ModelsShared",
                table: "FederatedInstances",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "SyncFeaturedOnly",
                table: "FederatedInstances",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "SyncIntervalMinutes",
                table: "FederatedInstances",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<long>(
                name: "TotalBytesTransferred",
                table: "FederatedInstances",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "Version",
                table: "FederatedInstances",
                type: "text",
                nullable: true);
        }
    }
}
