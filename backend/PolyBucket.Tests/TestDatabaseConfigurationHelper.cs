using Npgsql;

namespace PolyBucket.Tests;

internal static class TestDatabaseConfigurationHelper
{
    public static IReadOnlyDictionary<string, string?> GetDatabaseKeysFromConnectionString(string connectionString)
    {
        var b = new NpgsqlConnectionStringBuilder(connectionString);
        return new Dictionary<string, string?>
        {
            ["Database:Host"] = b.Host,
            ["Database:Port"] = b.Port.ToString(),
            ["Database:Name"] = b.Database,
            ["Database:Username"] = b.Username,
            ["Database:Password"] = b.Password ?? string.Empty,
            ["Database:SslMode"] = b.SslMode.ToString()
        };
    }
}
