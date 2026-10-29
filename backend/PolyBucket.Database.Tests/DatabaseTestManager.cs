using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using PolyBucket.Database.Infrastructure;
using PolyBucket.Database.Persistence;
using PolyBucket.Database.Settings;

namespace PolyBucket.Database.Tests;

internal static class DatabaseTestManager
{
    public static string GetConnectionString()
    {
        if (string.IsNullOrEmpty(DatabaseTestEnvironment.DefaultConnection))
        {
            throw new InvalidOperationException("Database test connection is not initialized.");
        }

        return DatabaseTestEnvironment.DefaultConnection;
    }

    public static DatabaseSettings GetDatabaseSettings()
    {
        var keys = DatabaseTestConfigurationHelper.GetDatabaseKeysFromConnectionString(GetConnectionString());
        return new DatabaseSettings
        {
            Host = keys["Database:Host"] ?? "",
            Port = int.Parse(keys["Database:Port"] ?? "5432"),
            Name = keys["Database:Name"] ?? "",
            Username = keys["Database:Username"] ?? "",
            Password = keys["Database:Password"],
            SslMode = keys["Database:SslMode"],
            EnsureDatabaseCreated = true,
            MaintenanceDatabase = "postgres"
        };
    }

    public static async Task EnsureMigratedAsync()
    {
        var settings = GetDatabaseSettings();
        var ensurer = new PostgresAppDatabaseEnsurer(Options.Create(settings));
        await ensurer.EnsureAppDatabaseExistsOrValidateForMigrationAsync(NullLogger<PostgresAppDatabaseEnsurer>.Instance);

        var options = new DbContextOptionsBuilder<PolyBucketDbContext>()
            .UseNpgsql(GetConnectionString(), b => b.MigrationsAssembly(typeof(PolyBucketDbContext).Assembly.GetName().Name!))
            .Options;

        await using var context = new PolyBucketDbContext(options);
        await context.Database.EnsureDeletedAsync();
        await context.Database.MigrateAsync();
    }

    public static PolyBucketDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<PolyBucketDbContext>()
            .UseNpgsql(GetConnectionString(), b => b.MigrationsAssembly(typeof(PolyBucketDbContext).Assembly.GetName().Name!))
            .Options;
        return new PolyBucketDbContext(options);
    }
}
