using Microsoft.Extensions.Options;
using Npgsql;

namespace PolyBucket.Api.Settings;

public sealed class DatabaseSettingsValidator : IValidateOptions<DatabaseSettings>
{
    public ValidateOptionsResult Validate(string? name, DatabaseSettings options)
    {
        if (string.IsNullOrWhiteSpace((options.Host ?? string.Empty).Trim()))
            return ValidateOptionsResult.Fail("Database:Host (or environment variable Database__Host) is required.");
        if (string.IsNullOrWhiteSpace((options.Name ?? string.Empty).Trim()))
            return ValidateOptionsResult.Fail("Database:Name (or environment variable Database__Name) is required.");
        if (string.IsNullOrWhiteSpace((options.Username ?? string.Empty).Trim()))
            return ValidateOptionsResult.Fail("Database:Username (or environment variable Database__Username) is required.");
        if (!string.IsNullOrWhiteSpace(options.SslMode) &&
            !Enum.TryParse<SslMode>(options.SslMode.Trim(), ignoreCase: true, out _))
        {
            return ValidateOptionsResult.Fail("Database:SslMode must be a valid Npgsql SslMode value (e.g. Disable, Require).");
        }
        return ValidateOptionsResult.Success;
    }
}
