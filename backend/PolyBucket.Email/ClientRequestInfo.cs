namespace PolyBucket.Email;

public sealed record ClientRequestInfo(string IpAddress, string UserAgent)
{
    public const string UnknownIp = "unknown";
    public const string UnknownUserAgent = "Unknown";

    public static ClientRequestInfo Unknown { get; } = new(UnknownIp, UnknownUserAgent);
}
