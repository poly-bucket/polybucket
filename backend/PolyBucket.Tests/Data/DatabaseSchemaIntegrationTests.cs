using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Data;

[Collection("TestCollection")]
public class DatabaseSchemaIntegrationTests : BaseIntegrationTest
{
    public DatabaseSchemaIntegrationTests(TestCollectionFixture testFixture) : base(testFixture)
    {
    }

    [Fact(DisplayName = "When migrations are applied, schema hardening indexes and constraints exist in PostgreSQL.")]
    public async Task SchemaHardening_IndexesExist()
    {
        // Arrange
        var expected = new[]
        {
            "IX_Users_Email",
            "IX_Users_Username",
            "IX_RefreshTokens_Token",
            "IX_Likes_ModelId_UserId_Active",
            "IX_ExternalAuthProviders_Provider_ExternalId",
            "IX_Models_AuthorId_CreatedAt_Active",
            "IX_Models_CreatedAt_PublicActive",
            "IX_Collections_OwnerId_CreatedAt_Active",
            "IX_Reports_IsResolved_CreatedAt",
            "IX_Reports_Type_TargetId_CreatedAt",
            "IX_EnhancedComments_Target_Thread_Active",
            "IX_Models_Name_trgm",
            "IX_Users_Username_trgm",
            "IX_Collections_Name_trgm",
            "FK_UserAuditLogs_Users_UserId",
            "FK_UserLogins_Users_UserId"
        };

        // Act
        var names = await DbContext.Database
            .SqlQueryRaw<string>(@"SELECT indexname AS ""Value"" FROM pg_indexes WHERE schemaname = 'public'")
            .ToListAsync();

        var constraints = await DbContext.Database
            .SqlQueryRaw<string>(@"SELECT conname AS ""Value"" FROM pg_constraint WHERE connamespace = 'public'::regnamespace")
            .ToListAsync();

        var present = new HashSet<string>(names.Concat(constraints));

        // Assert
        foreach (var name in expected)
        {
            present.Contains(name).ShouldBeTrue($"Expected schema object '{name}' to exist");
        }
    }
}
