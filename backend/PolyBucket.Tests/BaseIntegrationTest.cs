using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PolyBucket.Api;
using PolyBucket.Api.Data;
using PolyBucket.Tests.Factories;
using System;
using System.Threading.Tasks;
using Xunit;

namespace PolyBucket.Tests
{
    [Collection("TestCollection")]
    public abstract class BaseIntegrationTest : IAsyncDisposable
    {
        protected readonly TestWebApplicationFactory Factory;
        protected readonly TestCollectionFixture TestFixture;
        protected readonly TestUserFactory UserFactory;
        protected readonly TestModelFactory ModelFactory;
        protected readonly IServiceScope ServiceScope;
        protected readonly PolyBucketDbContext DbContext;

        protected BaseIntegrationTest(TestCollectionFixture testFixture)
        {
            TestFixture = testFixture;
            Factory = new TestWebApplicationFactory();
            ServiceScope = Factory.Services.CreateScope();
            DbContext = ServiceScope.ServiceProvider.GetRequiredService<PolyBucketDbContext>();
            UserFactory = new TestUserFactory(DbContext);
            ModelFactory = new TestModelFactory(DbContext);
        }

        protected async Task InitializeAsync()
        {
            await TestFixture.InitializeAsync();
            await CleanupDatabaseAsync();
            await TestDatabaseManager.EnsureTestDatabaseCreatedAsync();
        }

        private async Task CleanupDatabaseAsync()
        {
            // Clear all data but keep the schema - only truncate tables that exist
            try
            {
                // Clear all tables that might have data from seeding or previous tests
                await DbContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"Users\" CASCADE");
                await DbContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"Models\" CASCADE");
                await DbContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"ModelVersions\" CASCADE");
                await DbContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"ModelFiles\" CASCADE");
                await DbContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"Collections\" CASCADE");
                await DbContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"CollectionModels\" CASCADE");
                await DbContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"Comments\" CASCADE");
                await DbContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"UserSettings\" CASCADE");
                await DbContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"SystemSettings\" CASCADE");
                await DbContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"RolePermissions\" CASCADE");
                
                // Only truncate tables that exist (these might not exist in all schemas)
                try { await DbContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"Filaments\" CASCADE"); } catch { }
                try { await DbContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"Printers\" CASCADE"); } catch { }
                try { await DbContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"Reports\" CASCADE"); } catch { }
                try { await DbContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"UserRoles\" CASCADE"); } catch { }
                
                // Reset sequences if they exist
                try { await DbContext.Database.ExecuteSqlRawAsync("ALTER SEQUENCE IF EXISTS \"Users_Id_seq\" RESTART WITH 1"); } catch { }
                try { await DbContext.Database.ExecuteSqlRawAsync("ALTER SEQUENCE IF EXISTS \"Models_Id_seq\" RESTART WITH 1"); } catch { }
                try { await DbContext.Database.ExecuteSqlRawAsync("ALTER SEQUENCE IF EXISTS \"ModelVersions_Id_seq\" RESTART WITH 1"); } catch { }
                try { await DbContext.Database.ExecuteSqlRawAsync("ALTER SEQUENCE IF EXISTS \"ModelFiles_Id_seq\" RESTART WITH 1"); } catch { }
                try { await DbContext.Database.ExecuteSqlRawAsync("ALTER SEQUENCE IF EXISTS \"Collections_Id_seq\" RESTART WITH 1"); } catch { }
                try { await DbContext.Database.ExecuteSqlRawAsync("ALTER SEQUENCE IF EXISTS \"Comments_Id_seq\" RESTART WITH 1"); } catch { }
                try { await DbContext.Database.ExecuteSqlRawAsync("ALTER SEQUENCE IF EXISTS \"Filaments_Id_seq\" RESTART WITH 1"); } catch { }
                try { await DbContext.Database.ExecuteSqlRawAsync("ALTER SEQUENCE IF EXISTS \"Printers_Id_seq\" RESTART WITH 1"); } catch { }
                try { await DbContext.Database.ExecuteSqlRawAsync("ALTER SEQUENCE IF EXISTS \"Reports_Id_seq\" RESTART WITH 1"); } catch { }
                try { await DbContext.Database.ExecuteSqlRawAsync("ALTER SEQUENCE IF EXISTS \"UserSettings_Id_seq\" RESTART WITH 1"); } catch { }
                try { await DbContext.Database.ExecuteSqlRawAsync("ALTER SEQUENCE IF EXISTS \"SystemSettings_Id_seq\" RESTART WITH 1"); } catch { }
            }
            catch (Exception ex)
            {
                // Log the error but don't fail the test
                Console.WriteLine($"Warning: Database cleanup failed: {ex.Message}");
            }
        }

        public async ValueTask DisposeAsync()
        {
            ServiceScope?.Dispose();
            Factory?.Dispose();
        }
    }
} 