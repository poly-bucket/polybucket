using DotNet.Testcontainers.Images;
using Npgsql;
using Testcontainers.PostgreSql;
using Xunit;
using Xunit;

namespace PolyBucket.Database.Tests;

[CollectionDefinition("DatabaseTestCollection")]
public class DatabaseTestCollection : ICollectionFixture<DatabaseTestCollectionFixture>
{
}

public class DatabaseTestCollectionFixture : IAsyncLifetime
{
    private PostgreSqlContainer? _postgresContainer;

    public async Task InitializeAsync()
    {
        _postgresContainer = new PostgreSqlBuilder("postgres:16-alpine")
            .WithImagePullPolicy(PullPolicy.Missing)
            .WithDatabase("postgres")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();

        await _postgresContainer.StartAsync();

        var builder = new NpgsqlConnectionStringBuilder(_postgresContainer.GetConnectionString())
        {
            Database = "polybucket_database_test",
            SslMode = SslMode.Disable
        };
        DatabaseTestEnvironment.DefaultConnection = builder.ConnectionString;
        await DatabaseTestManager.EnsureMigratedAsync();
    }

    public async Task DisposeAsync()
    {
        if (_postgresContainer is not null)
        {
            await _postgresContainer.DisposeAsync();
        }
    }
}
