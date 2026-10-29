using Microsoft.AspNetCore.Http;
using PolyBucket.Email;

namespace PolyBucket.Api.Common.Http;

public static class ClientRequestInfoMapper
{
    private const int MaxUserAgentLength = 512;

    public static ClientRequestInfo From(HttpContext? httpContext)
    {
        if (httpContext == null)
        {
            return ClientRequestInfo.Unknown;
        }

        var ip = httpContext.Connection.RemoteIpAddress;
        var ipText = ip == null
            ? ClientRequestInfo.UnknownIp
            : (ip.IsIPv4MappedToIPv6 ? ip.MapToIPv4() : ip).ToString();

        var userAgent = httpContext.Request.Headers.UserAgent.ToString();
        if (string.IsNullOrWhiteSpace(userAgent))
        {
            userAgent = ClientRequestInfo.UnknownUserAgent;
        }
        else if (userAgent.Length > MaxUserAgentLength)
        {
            userAgent = userAgent[..MaxUserAgentLength];
        }

        return new ClientRequestInfo(ipText, userAgent);
    }
}
