using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace PolyBucket.Api.Data;

public sealed class PostgresAppDatabaseEnsurer(IConfiguration configuration)
{
    private const string DefaultConnectionName = "DefaultConnection";
    private static readonly Regex SafeIdentifier = new("^[a-z0-9_]+$", RegexOptions.Compiled);

    public async Task EnsureAppDatabaseExistsOrValidateForMigrationAsync(
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        var ensureCreate = configuration.GetValue("Database:EnsureDatabaseCreated", true);
        var connectionString = configuration.GetConnectionString(DefaultConnectionName);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Connection string 'DefaultConnection' is not configured. Cannot prepare the application database.");
        }

        if (!TryParseAppBuilder(connectionString, out var appBuilder))
        {
            throw new InvalidOperationException(
                "Failed to parse PostgreSQL connection string. Cannot prepare the application database.");
        }

        var appDatabase = appBuilder.Database;
        if (string.IsNullOrEmpty(appDatabase))
        {
            throw new InvalidOperationException(
                "The connection string does not specify a database name. Add a Database= value.");
        }

        if (!SafeIdentifier.IsMatch(appDatabase))
        {
            throw new InvalidOperationException(
                "The database name in the connection string must be a simple unquoted identifier (a-z, 0-9, _).");
        }

        logger.LogInformation(
            "Checking whether PostgreSQL database \"{AppDatabase}\" exists (host {Host}, port {Port})",
            appDatabase,
            appBuilder.Host,
            appBuilder.Port);

        if (ensureCreate)
        {
            await CreateDatabaseIfMissingAsync(logger, appBuilder, appDatabase, cancellationToken).ConfigureAwait(false);
            return;
        }

        logger.LogInformation(
            "Database auto-create is off (Database:EnsureDatabaseCreated=false). Verifying the application database is reachable (host {Host}, port {Port}, database {AppDatabase})",
            appBuilder.Host,
            appBuilder.Port,
            appDatabase);

        try
        {
            await using var appConn = new NpgsqlConnection(connectionString);
            await appConn.OpenAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "The application database \"{AppDatabase}\" is not reachable while auto-create is disabled. " +
                "Pre-create the catalog and grant access, or set Database:EnsureDatabaseCreated to true. " +
                "If the connection string is wrong, fix the connection; this is a provisioning or configuration error, not a migration failure.",
                appDatabase);
            throw;
        }
    }

    private static bool TryParseAppBuilder(
        string connectionString,
        out NpgsqlConnectionStringBuilder appBuilder)
    {
        try
        {
            appBuilder = new NpgsqlConnectionStringBuilder(connectionString);
        }
        catch
        {
            appBuilder = null!;
            return false;
        }

        return true;
    }

    private async Task CreateDatabaseIfMissingAsync(
        ILogger logger,
        NpgsqlConnectionStringBuilder appBuilder,
        string appDatabase,
        CancellationToken cancellationToken)
    {
        var maintenanceName = configuration["Database:MaintenanceDatabase"]?.Trim() ?? "postgres";
        if (!SafeIdentifier.IsMatch(maintenanceName))
        {
            throw new InvalidOperationException("Database:MaintenanceDatabase must be a simple identifier (a-z, 0-9, _).");
        }

        var maintBuilder = new NpgsqlConnectionStringBuilder(appBuilder.ConnectionString)
        {
            Database = maintenanceName
        };

        await using (var maint = new NpgsqlConnection(maintBuilder.ConnectionString))
        {
            await maint.OpenAsync(cancellationToken).ConfigureAwait(false);

            await using var checkCmd = new NpgsqlCommand(
                "SELECT 1 FROM pg_database WHERE datname = $1",
                maint);
            checkCmd.Parameters.AddWithValue(appDatabase);
            var exists = await checkCmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false) != null;
            if (exists)
            {
                logger.LogDebug("Application database \"{AppDatabase}\" already exists; skipping create", appDatabase);
                return;
            }

            logger.LogInformation("Creating application database \"{AppDatabase}\"", appDatabase);
            var sql = "CREATE DATABASE \"" + appDatabase.Replace("\"", "\"\"") + "\"";
            try
            {
                await using var createCmd = new NpgsqlCommand(sql, maint);
                createCmd.CommandTimeout = 0;
                await createCmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (PostgresException pex) when (IsDatabaseAlreadyExists(pex))
            {
                logger.LogWarning("Database \"{AppDatabase}\" was created by another process; continuing", appDatabase);
                return;
            }

            logger.LogInformation("Created application database \"{AppDatabase}\" on {Host} port {Port} (maintenance database {Maint})",
                appDatabase,
                maintBuilder.Host,
                maintBuilder.Port,
                maintenanceName);
        }
    }

    private static bool IsDatabaseAlreadyExists(PostgresException pex) =>
        pex.SqlState == "42P12"
        || pex.SqlState == "42P04"
        || pex.Message.Contains("already exists", StringComparison.OrdinalIgnoreCase);
}
