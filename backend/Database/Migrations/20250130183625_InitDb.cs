using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Database.Migrations
{
    /// <inheritdoc />
    public partial class InitDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "filaments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid(36)", nullable: false, collation: "ascii_general_ci"),
                    manufacturer = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    type = table.Column<int>(type: "int", nullable: false),
                    color = table.Column<string>(type: "varchar(50)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    diameter = table.Column<string>(type: "varchar(10)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_filaments", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "print_settings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid(36)", nullable: false, collation: "ascii_general_ci"),
                    supports = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    layer_height = table.Column<decimal>(type: "decimal(5,3)", nullable: false),
                    wall_loops = table.Column<int>(type: "int", nullable: false),
                    infill_percentage = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_print_settings", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "printers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid(36)", nullable: false, collation: "ascii_general_ci"),
                    manufacturer = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    model = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    build_volume_x = table.Column<int>(type: "int", nullable: false),
                    build_volume_y = table.Column<int>(type: "int", nullable: false),
                    build_volume_z = table.Column<int>(type: "int", nullable: false),
                    max_bed_temp = table.Column<int>(type: "int", nullable: false),
                    max_nozzle_temp = table.Column<int>(type: "int", nullable: false),
                    has_heated_bed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    has_heated_chamber = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    has_enclosure = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    default_nozzle_diameter = table.Column<decimal>(type: "decimal(4,2)", nullable: false),
                    extruder_count = table.Column<int>(type: "int", nullable: false),
                    extruder_type = table.Column<string>(type: "varchar(50)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    connectivity = table.Column<string>(type: "json", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    supported_materials = table.Column<string>(type: "json", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    price_usd = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    release_year = table.Column<int>(type: "int", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    type = table.Column<string>(type: "varchar(50)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    has_auto_leveling = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    has_filament_sensor = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    has_power_loss_continue = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    max_print_speed = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    website_url = table.Column<string>(type: "varchar(2048)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedById = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: false),
                    UpdatedById = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    updated_at = table.Column<DateTime>(type: "timestamp", nullable: true),
                    DeletedById = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    DeletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_printers", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "user_logins",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid(36)", nullable: false, collation: "ascii_general_ci"),
                    email = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Successful = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    ip_address = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    user_agent = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_logins", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid(36)", nullable: false, collation: "ascii_general_ci"),
                    email = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    username = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    first_name = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    last_name = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    salt = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    password_hash = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    country = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Role = table.Column<string>(type: "varchar(20)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedById = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedById = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DeletedById = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    DeletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "models",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid(36)", nullable: false, collation: "ascii_general_ci"),
                    name = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    description = table.Column<string>(type: "text", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    license = table.Column<int>(type: "int", nullable: true),
                    privacy = table.Column<int>(type: "int", nullable: false),
                    categories = table.Column<string>(type: "json", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ai_generated = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    wip = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    nsfw = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    is_remix = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_by_id1 = table.Column<Guid>(type: "uuid(36)", nullable: false, collation: "ascii_general_ci"),
                    created_by_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_by_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    deleted_by_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    deleted_at = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_models", x => x.id);
                    table.ForeignKey(
                        name: "FK_models_users_created_by_id1",
                        column: x => x.created_by_id1,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "comments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid(36)", nullable: false, collation: "ascii_general_ci"),
                    content = table.Column<string>(type: "text", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    likes = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    dislikes = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    model_id = table.Column<Guid>(type: "uuid(36)", nullable: false, collation: "ascii_general_ci"),
                    author_id = table.Column<Guid>(type: "uuid(36)", nullable: false, collation: "ascii_general_ci"),
                    created_by_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_by_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    deleted_by_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    deleted_at = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_comments", x => x.id);
                    table.ForeignKey(
                        name: "FK_comments_models_model_id",
                        column: x => x.model_id,
                        principalTable: "models",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_comments_users_author_id",
                        column: x => x.author_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "files",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid(36)", nullable: false, collation: "ascii_general_ci"),
                    name = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    path = table.Column<string>(type: "varchar(1024)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    extension = table.Column<string>(type: "varchar(10)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    mime_type = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    size = table.Column<long>(type: "bigint", nullable: false),
                    model_id = table.Column<Guid>(type: "uuid(36)", nullable: true, collation: "ascii_general_ci"),
                    created_by_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_by_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    deleted_by_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    deleted_at = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_files", x => x.id);
                    table.ForeignKey(
                        name: "FK_files_models_model_id",
                        column: x => x.model_id,
                        principalTable: "models",
                        principalColumn: "id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_comments_author_id",
                table: "comments",
                column: "author_id");

            migrationBuilder.CreateIndex(
                name: "IX_comments_model_id",
                table: "comments",
                column: "model_id");

            migrationBuilder.CreateIndex(
                name: "IX_files_model_id",
                table: "files",
                column: "model_id");

            migrationBuilder.CreateIndex(
                name: "IX_models_created_by_id1",
                table: "models",
                column: "created_by_id1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "comments");

            migrationBuilder.DropTable(
                name: "filaments");

            migrationBuilder.DropTable(
                name: "files");

            migrationBuilder.DropTable(
                name: "print_settings");

            migrationBuilder.DropTable(
                name: "printers");

            migrationBuilder.DropTable(
                name: "user_logins");

            migrationBuilder.DropTable(
                name: "models");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
