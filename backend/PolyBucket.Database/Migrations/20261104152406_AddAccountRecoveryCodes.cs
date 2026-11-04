using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PolyBucket.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddAccountRecoveryCodes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProtectedPendingRecoveryCodesPayload",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RecoveryCodesAcknowledgedAt",
                table: "Users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RecoveryCodesGeneratedAt",
                table: "Users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AccountRecoveryCodes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProtectedCodeHash = table.Column<string>(type: "text", nullable: false),
                    IsUsed = table.Column<bool>(type: "boolean", nullable: false),
                    UsedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedById = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedById = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountRecoveryCodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccountRecoveryCodes_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccountRecoveryCodes_UserId_IsUsed",
                table: "AccountRecoveryCodes",
                columns: new[] { "UserId", "IsUsed" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccountRecoveryCodes");

            migrationBuilder.DropColumn(
                name: "ProtectedPendingRecoveryCodesPayload",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "RecoveryCodesAcknowledgedAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "RecoveryCodesGeneratedAt",
                table: "Users");
        }
    }
}
