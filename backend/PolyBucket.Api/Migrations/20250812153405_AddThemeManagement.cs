using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class AddThemeManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Themes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Themes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ThemeColors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ThemeId = table.Column<int>(type: "integer", nullable: false),
                    Primary = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    PrimaryLight = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    PrimaryDark = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    Secondary = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    SecondaryLight = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    SecondaryDark = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    Accent = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    AccentLight = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    AccentDark = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    BackgroundPrimary = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    BackgroundSecondary = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    BackgroundTertiary = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThemeColors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ThemeColors_Themes_ThemeId",
                        column: x => x.ThemeId,
                        principalTable: "Themes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ThemeColors_ThemeId",
                table: "ThemeColors",
                column: "ThemeId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Themes_Name",
                table: "Themes",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ThemeColors");

            migrationBuilder.DropTable(
                name: "Themes");
        }
    }
}
