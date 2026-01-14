namespace PolyBucket.Api.Settings;

public class EnvironmentSettings
{
    public string JwtIssuer { get; set; } = string.Empty;
    public string JwtAudience { get; set; } = string.Empty;
    public string JwtSecret { get; set; } = string.Empty;
} 