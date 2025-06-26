namespace PolyBucket.Api.Settings;

public class SecuritySettings
{
    public string JwtIssuer { get; set; } = string.Empty;
    public string JwtAudience { get; set; } = string.Empty;
    public string JwtSecret { get; set; } = string.Empty;
} 