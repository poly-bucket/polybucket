using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;
using Npgsql;
using PolyBucket.Api.Data;
using Testcontainers.PostgreSql;
using Xunit;
using LogAbstractions = Microsoft.Extensions.Logging.Abstractions;

namespace PolyBucket.Tests
{
    [CollectionDefinition("TestCollection")]
    public class TestCollection : ICollectionFixture<TestCollectionFixture>
    {
    }

    public class TestCollectionFixture : IAsyncLifetime
    {
        private PostgreSqlContainer? _container;

        public async Task InitializeAsync()
        {
            _container = new PostgreSqlBuilder("postgres:16-alpine")
                .WithDatabase("postgres")
                .WithUsername("postgres")
                .WithPassword("postgres")
                .Build();

            await _container.StartAsync().ConfigureAwait(false);

            var builder = new NpgsqlConnectionStringBuilder(_container.GetConnectionString())
            {
                Database = "polybucket_test",
                SslMode = SslMode.Disable
            };
            TestEnvironment.DefaultConnection = builder.ConnectionString;

            var configuration = BuildConfigurationForEnsurer();
            var ensurer = new PostgresAppDatabaseEnsurer(configuration);
            var logger = LogAbstractions.NullLogger<PostgresAppDatabaseEnsurer>.Instance;
            await ensurer.EnsureAppDatabaseExistsOrValidateForMigrationAsync(logger).ConfigureAwait(false);
            await TestDatabaseManager.EnsureTestDatabaseCreatedAsync().ConfigureAwait(false);
        }

        public async Task DisposeAsync()
        {
            if (_container is not null)
            {
                await _container.DisposeAsync().ConfigureAwait(false);
            }
        }

        private static IConfiguration BuildConfigurationForEnsurer()
        {
            return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.Test.json", optional: true)
                .AddInMemoryCollection(
                    new Dictionary<string, string?>
                    {
                        ["ConnectionStrings:DefaultConnection"] = TestEnvironment.DefaultConnection,
                        ["Database:EnsureDatabaseCreated"] = "true",
                        ["Database:MaintenanceDatabase"] = "postgres"
                    })
                .Build();
        }
    }
}
