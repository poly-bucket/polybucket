using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDefaultModelPrivacyToEnumWithDataConversion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add a temporary column
            migrationBuilder.AddColumn<int>(
                name: "DefaultModelPrivacy_Temp",
                table: "SystemSetups",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            // Convert data from string to integer
            migrationBuilder.Sql(@"
                UPDATE ""SystemSetups"" 
                SET ""DefaultModelPrivacy_Temp"" = CASE 
                    WHEN ""DefaultModelPrivacy"" = 'public' THEN 1
                    WHEN ""DefaultModelPrivacy"" = 'private' THEN 2
                    WHEN ""DefaultModelPrivacy"" = 'unlisted' THEN 3
                    ELSE 1
                END
            ");

            // Drop the old column
            migrationBuilder.DropColumn(
                name: "DefaultModelPrivacy",
                table: "SystemSetups");

            // Rename the temporary column to the original name
            migrationBuilder.RenameColumn(
                name: "DefaultModelPrivacy_Temp",
                table: "SystemSetups",
                newName: "DefaultModelPrivacy");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Add a temporary string column
            migrationBuilder.AddColumn<string>(
                name: "DefaultModelPrivacy_Temp",
                table: "SystemSetups",
                type: "text",
                nullable: false,
                defaultValue: "public");

            // Convert data from integer back to string
            migrationBuilder.Sql(@"
                UPDATE ""SystemSetups"" 
                SET ""DefaultModelPrivacy_Temp"" = CASE 
                    WHEN ""DefaultModelPrivacy"" = 1 THEN 'public'
                    WHEN ""DefaultModelPrivacy"" = 2 THEN 'private'
                    WHEN ""DefaultModelPrivacy"" = 3 THEN 'unlisted'
                    ELSE 'public'
                END
            ");

            // Drop the integer column
            migrationBuilder.DropColumn(
                name: "DefaultModelPrivacy",
                table: "SystemSetups");

            // Rename the temporary column to the original name
            migrationBuilder.RenameColumn(
                name: "DefaultModelPrivacy_Temp",
                table: "SystemSetups",
                newName: "DefaultModelPrivacy");
        }
    }
}
