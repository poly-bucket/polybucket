using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class FixCanLoginForExistingUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Set CanLogin = true for all existing (non-federated) users
            migrationBuilder.Sql(
                @"UPDATE ""Users"" 
                  SET ""CanLogin"" = true 
                  WHERE ""IsFederated"" = false;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revert to false (though this shouldn't normally be needed)
            migrationBuilder.Sql(
                @"UPDATE ""Users"" 
                  SET ""CanLogin"" = false 
                  WHERE ""IsFederated"" = false;");
        }
    }
}

