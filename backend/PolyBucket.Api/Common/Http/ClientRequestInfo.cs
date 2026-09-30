using Microsoft.AspNetCore.Http;

namespace PolyBucket.Api.Common.Http;

public sealed record ClientRequestInfo(string IpAddress, string UserAgent)
{
    public const string UnknownIp = "unknown";
    public const string UnknownUserAgent = "Unknown";
    private const int MaxUserAgentLength = 512;

    public static ClientRequestInfo Unknown { get; } = new(UnknownIp, UnknownUserAgent);

    public static ClientRequestInfo From(HttpContext? httpContext)
    {
        if (httpContext == null)
        {
            return Unknown;
        }

        var ip = httpContext.Connection.RemoteIpAddress;
        var ipText = ip == null
            ? UnknownIp
            : (ip.IsIPv4MappedToIPv6 ? ip.MapToIPv4() : ip).ToString();

        var userAgent = httpContext.Request.Headers.UserAgent.ToString();
        if (string.IsNullOrWhiteSpace(userAgent))
        {
            userAgent = UnknownUserAgent;
        }
        else if (userAgent.Length > MaxUserAgentLength)
        {
            userAgent = userAgent[..MaxUserAgentLength];
        }

        return new ClientRequestInfo(ipText, userAgent);
    }
}
