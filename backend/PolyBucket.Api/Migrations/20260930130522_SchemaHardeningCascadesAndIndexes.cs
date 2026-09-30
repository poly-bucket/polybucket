using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class SchemaHardeningCascadesAndIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Fail fast if duplicate data would break new unique constraints.
            // Cleanup examples:
            //   SELECT "Email", COUNT(*) FROM "Users" GROUP BY "Email" HAVING COUNT(*) > 1;
            //   SELECT "ModelId", "UserId", COUNT(*) FROM "Likes" WHERE "DeletedAt" IS NULL GROUP BY 1,2 HAVING COUNT(*) > 1;
            migrationBuilder.Sql("""
                DO $pre$
                BEGIN
                    IF EXISTS (SELECT 1 FROM "Users" GROUP BY "Email" HAVING COUNT(*) > 1) THEN
                        RAISE EXCEPTION 'SchemaHardening: duplicate Users.Email values exist';
                    END IF;
                    IF EXISTS (SELECT 1 FROM "Users" GROUP BY "Username" HAVING COUNT(*) > 1) THEN
                        RAISE EXCEPTION 'SchemaHardening: duplicate Users.Username values exist';
                    END IF;
                    IF EXISTS (SELECT 1 FROM "RefreshTokens" GROUP BY "Token" HAVING COUNT(*) > 1) THEN
                        RAISE EXCEPTION 'SchemaHardening: duplicate RefreshTokens.Token values exist';
                    END IF;
                    IF EXISTS (SELECT 1 FROM "ExternalAuthProviders" GROUP BY "Provider", "ExternalId" HAVING COUNT(*) > 1) THEN
                        RAISE EXCEPTION 'SchemaHardening: duplicate ExternalAuthProviders (Provider, ExternalId) exist';
                    END IF;
                    IF EXISTS (
                        SELECT 1 FROM "Likes"
                        WHERE "DeletedAt" IS NULL
                        GROUP BY "ModelId", "UserId"
                        HAVING COUNT(*) > 1
                    ) THEN
                        RAISE EXCEPTION 'SchemaHardening: duplicate active Likes (ModelId, UserId) exist';
                    END IF;
                END
                $pre$;
                """);

            migrationBuilder.DropForeignKey(
                name: "FK_ModerationAuditLogs_Users_PerformedByUserId",
                table: "ModerationAuditLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_RolePermissions_Users_GrantedByUserId",
                table: "RolePermissions");

            migrationBuilder.DropForeignKey(
                name: "FK_UserLogins_Users_UserId",
                table: "UserLogins");

            migrationBuilder.DropForeignKey(
                name: "FK_UserPermissions_Users_GrantedByUserId",
                table: "UserPermissions");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Users_BannedByUserId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_RefreshTokens_UserId",
                table: "RefreshTokens");

            migrationBuilder.DropIndex(
                name: "IX_Models_AuthorId",
                table: "Models");

            migrationBuilder.DropIndex(
                name: "IX_Likes_ModelId",
                table: "Likes");

            migrationBuilder.DropIndex(
                name: "IX_FederationAuditLogs_FederatedInstanceId",
                table: "FederationAuditLogs");

            migrationBuilder.DropIndex(
                name: "IX_EnhancedComments_TargetType_TargetId_CreatedAt",
                table: "EnhancedComments");

            migrationBuilder.DropColumn(
                name: "BannedById",
                table: "Users");

            migrationBuilder.AlterColumn<Guid>(
                name: "PerformedByUserId",
                table: "ModerationAuditLogs",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.CreateIndex(
                name: "IX_Users_BannedAt_WhenBanned",
                table: "Users",
                column: "BannedAt",
                descending: new bool[0],
                filter: "\"IsBanned\" = true");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserAuditLogs_PerformedByUserId",
                table: "UserAuditLogs",
                column: "PerformedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_IsResolved_CreatedAt",
                table: "Reports",
                columns: new[] { "IsResolved", "CreatedAt" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_Reports_Type_TargetId_CreatedAt",
                table: "Reports",
                columns: new[] { "Type", "TargetId", "CreatedAt" },
                descending: new[] { false, false, true });

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_Token",
                table: "RefreshTokens",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId_CreatedAt_Active",
                table: "RefreshTokens",
                columns: new[] { "UserId", "CreatedAt" },
                descending: new[] { false, true },
                filter: "\"RevokedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PasswordResetTokens_Email_IsUsed",
                table: "PasswordResetTokens",
                columns: new[] { "Email", "IsUsed" });

            migrationBuilder.CreateIndex(
                name: "IX_Models_AuthorId_CreatedAt_Active",
                table: "Models",
                columns: new[] { "AuthorId", "CreatedAt" },
                descending: new[] { false, true },
                filter: "\"DeletedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Models_CreatedAt_PublicActive",
                table: "Models",
                column: "CreatedAt",
                descending: new bool[0],
                filter: "\"DeletedAt\" IS NULL AND \"Privacy\" = 1");

            migrationBuilder.CreateIndex(
                name: "IX_Likes_ModelId_UserId_Active",
                table: "Likes",
                columns: new[] { "ModelId", "UserId" },
                unique: true,
                filter: "\"DeletedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_FederationAuditLogs_InstanceId_EventTimestamp",
                table: "FederationAuditLogs",
                columns: new[] { "FederatedInstanceId", "EventTimestamp" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_ExternalAuthProviders_Provider_ExternalId",
                table: "ExternalAuthProviders",
                columns: new[] { "Provider", "ExternalId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EnhancedComments_TargetType_TargetId_CreatedAt",
                table: "EnhancedComments",
                columns: new[] { "TargetType", "TargetId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_EnhancedComments_Target_Thread_Active",
                table: "EnhancedComments",
                columns: new[] { "TargetType", "TargetId", "CreatedAt" },
                descending: new[] { false, false, true },
                filter: "\"ParentCommentId\" IS NULL AND NOT \"IsHidden\"");

            migrationBuilder.CreateIndex(
                name: "IX_Collections_OwnerId_CreatedAt_Active",
                table: "Collections",
                columns: new[] { "OwnerId", "CreatedAt" },
                descending: new[] { false, true },
                filter: "\"DeletedAt\" IS NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_ModerationAuditLogs_Users_PerformedByUserId",
                table: "ModerationAuditLogs",
                column: "PerformedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_RolePermissions_Users_GrantedByUserId",
                table: "RolePermissions",
                column: "GrantedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_UserAuditLogs_Users_PerformedByUserId",
                table: "UserAuditLogs",
                column: "PerformedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_UserAuditLogs_Users_UserId",
                table: "UserAuditLogs",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserLogins_Users_UserId",
                table: "UserLogins",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_UserPermissions_Users_GrantedByUserId",
                table: "UserPermissions",
                column: "GrantedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Users_BannedByUserId",
                table: "Users",
                column: "BannedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS pg_trgm;");
            migrationBuilder.Sql("""
                CREATE INDEX "IX_Models_Name_trgm"
                    ON "Models" USING gin (lower("Name") gin_trgm_ops);
                CREATE INDEX "IX_Users_Username_trgm"
                    ON "Users" USING gin (lower("Username") gin_trgm_ops);
                CREATE INDEX "IX_Collections_Name_trgm"
                    ON "Collections" USING gin (lower("Name") gin_trgm_ops);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP INDEX IF EXISTS "IX_Collections_Name_trgm";
                DROP INDEX IF EXISTS "IX_Users_Username_trgm";
                DROP INDEX IF EXISTS "IX_Models_Name_trgm";
                """);

            migrationBuilder.DropForeignKey(
                name: "FK_ModerationAuditLogs_Users_PerformedByUserId",
                table: "ModerationAuditLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_RolePermissions_Users_GrantedByUserId",
                table: "RolePermissions");

            migrationBuilder.DropForeignKey(
                name: "FK_UserAuditLogs_Users_PerformedByUserId",
                table: "UserAuditLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_UserAuditLogs_Users_UserId",
                table: "UserAuditLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_UserLogins_Users_UserId",
                table: "UserLogins");

            migrationBuilder.DropForeignKey(
                name: "FK_UserPermissions_Users_GrantedByUserId",
                table: "UserPermissions");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Users_BannedByUserId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_BannedAt_WhenBanned",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_Email",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_Username",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_UserAuditLogs_PerformedByUserId",
                table: "UserAuditLogs");

            migrationBuilder.DropIndex(
                name: "IX_Reports_IsResolved_CreatedAt",
                table: "Reports");

            migrationBuilder.DropIndex(
                name: "IX_Reports_Type_TargetId_CreatedAt",
                table: "Reports");

            migrationBuilder.DropIndex(
                name: "IX_RefreshTokens_Token",
                table: "RefreshTokens");

            migrationBuilder.DropIndex(
                name: "IX_RefreshTokens_UserId_CreatedAt_Active",
                table: "RefreshTokens");

            migrationBuilder.DropIndex(
                name: "IX_PasswordResetTokens_Email_IsUsed",
                table: "PasswordResetTokens");

            migrationBuilder.DropIndex(
                name: "IX_Models_AuthorId_CreatedAt_Active",
                table: "Models");

            migrationBuilder.DropIndex(
                name: "IX_Models_CreatedAt_PublicActive",
                table: "Models");

            migrationBuilder.DropIndex(
                name: "IX_Likes_ModelId_UserId_Active",
                table: "Likes");

            migrationBuilder.DropIndex(
                name: "IX_FederationAuditLogs_InstanceId_EventTimestamp",
                table: "FederationAuditLogs");

            migrationBuilder.DropIndex(
                name: "IX_ExternalAuthProviders_Provider_ExternalId",
                table: "ExternalAuthProviders");

            migrationBuilder.DropIndex(
                name: "IX_EnhancedComments_Target_Thread_Active",
                table: "EnhancedComments");

            migrationBuilder.DropIndex(
                name: "IX_EnhancedComments_TargetType_TargetId_CreatedAt",
                table: "EnhancedComments");

            migrationBuilder.DropIndex(
                name: "IX_Collections_OwnerId_CreatedAt_Active",
                table: "Collections");

            migrationBuilder.AddColumn<Guid>(
                name: "BannedById",
                table: "Users",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "PerformedByUserId",
                table: "ModerationAuditLogs",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId",
                table: "RefreshTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Models_AuthorId",
                table: "Models",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_Likes_ModelId",
                table: "Likes",
                column: "ModelId");

            migrationBuilder.CreateIndex(
                name: "IX_FederationAuditLogs_FederatedInstanceId",
                table: "FederationAuditLogs",
                column: "FederatedInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_EnhancedComments_TargetType_TargetId_CreatedAt",
                table: "EnhancedComments",
                columns: new[] { "TargetType", "TargetId", "CreatedAt" });

            migrationBuilder.AddForeignKey(
                name: "FK_ModerationAuditLogs_Users_PerformedByUserId",
                table: "ModerationAuditLogs",
                column: "PerformedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RolePermissions_Users_GrantedByUserId",
                table: "RolePermissions",
                column: "GrantedByUserId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserLogins_Users_UserId",
                table: "UserLogins",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserPermissions_Users_GrantedByUserId",
                table: "UserPermissions",
                column: "GrantedByUserId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Users_BannedByUserId",
                table: "Users",
                column: "BannedByUserId",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
