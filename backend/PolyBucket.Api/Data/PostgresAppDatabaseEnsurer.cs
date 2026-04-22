using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using PolyBucket.Api.Settings;

namespace PolyBucket.Api.Data;

public sealed class PostgresAppDatabaseEnsurer(IOptions<DatabaseSettings> databaseOptions)
{
    private static readonly Regex SafeIdentifier = new("^[a-z0-9_]+$", RegexOptions.Compiled);

    public async Task EnsureAppDatabaseExistsOrValidateForMigrationAsync(
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        var settings = databaseOptions.Value;
        var ensureCreate = settings.EnsureDatabaseCreated;
        string connectionString;
        try
        {
            connectionString = settings.BuildConnectionString();
        }
        catch (InvalidOperationException ex)
        {
            throw new InvalidOperationException(
                "Database connection could not be built from the Database section. Ensure Database:Host, Database:Name, and Database:Username are set.",
                ex);
        }

        if (!TryParseAppBuilder(connectionString, out var appBuilder))
        {
            throw new InvalidOperationException(
                "Failed to parse PostgreSQL connection parameters from the Database section. Cannot prepare the application database.");
        }

        var appDatabase = appBuilder.Database;
        if (string.IsNullOrEmpty(appDatabase))
        {
            throw new InvalidOperationException(
                "The Database:Name (catalog) is not set. Set Database:Name in configuration.");
        }

        if (!SafeIdentifier.IsMatch(appDatabase))
        {
            throw new InvalidOperationException(
                "The database name in Database:Name must be a simple unquoted identifier (a-z, 0-9, _).");
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
                "If values in the Database section are wrong, fix configuration; this is a provisioning or configuration error, not a migration failure.",
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
        var maintenanceName = databaseOptions.Value.MaintenanceDatabase?.Trim() ?? "postgres";
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
