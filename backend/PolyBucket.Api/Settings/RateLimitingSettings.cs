namespace PolyBucket.Api.Settings;

public class RateLimitingSettings
{
    public const string SectionName = "RateLimiting";

    public bool Enabled { get; set; } = true;
    public RateLimitPolicySettings AuthStrict { get; set; } = new() { PermitLimit = 10, WindowSeconds = 60 };
    public RateLimitPolicySettings AuthStandard { get; set; } = new() { PermitLimit = 60, WindowSeconds = 60 };
}

public class RateLimitPolicySettings
{
    public int PermitLimit { get; set; }
    public int WindowSeconds { get; set; }
}

public class ForwardedHeadersSettings
{
    public const string SectionName = "ForwardedHeaders";

    public bool Enabled { get; set; } = true;
    public string[] KnownProxies { get; set; } = [];
    public string[] KnownNetworks { get; set; } = [];
    public int ForwardLimit { get; set; } = 1;
}
