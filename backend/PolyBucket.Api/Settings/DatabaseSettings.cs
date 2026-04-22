using Npgsql;

namespace PolyBucket.Api.Settings;

public class DatabaseSettings
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string? Password { get; set; }
    public string? SslMode { get; set; }
    public bool EnsureDatabaseCreated { get; set; } = true;
    public string MaintenanceDatabase { get; set; } = "postgres";
    public bool SkipHostDatabaseInitialization { get; set; }

    public string BuildConnectionString()
    {
        var host = (Host ?? string.Empty).Trim();
        var name = (Name ?? string.Empty).Trim();
        var user = (Username ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(name) || string.IsNullOrEmpty(user))
        {
            throw new InvalidOperationException(
                "Database:Host, Database:Name, and Database:Username (or environment Database__*) must be configured. See the Database section in configuration.");
        }

        var port = Port > 0 ? Port : 5432;
        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = host,
            Port = port,
            Database = name,
            Username = user,
            Password = Password ?? string.Empty
        };

        if (!string.IsNullOrWhiteSpace(SslMode) &&
            Enum.TryParse<SslMode>(SslMode.Trim(), ignoreCase: true, out var ssl))
        {
            builder.SslMode = ssl;
        }

        return builder.ConnectionString;
    }
}
