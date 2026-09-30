using System.Net;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using PolyBucket.Api.Common.Http;
using PolyBucket.Api.Settings;

namespace PolyBucket.Api.Extensions;

public static class RequestSecurityServiceCollectionExtensions
{
    public const string AuthStrictPolicy = "auth-strict";
    public const string AuthStandardPolicy = "auth-standard";

    public static IServiceCollection AddPolyBucketForwardedHeaders(this IServiceCollection services, IConfiguration configuration)
    {
        var settings = configuration.GetSection(ForwardedHeadersSettings.SectionName).Get<ForwardedHeadersSettings>()
            ?? new ForwardedHeadersSettings();

        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
            options.ForwardLimit = settings.ForwardLimit <= 0 ? null : settings.ForwardLimit;

            foreach (var proxy in settings.KnownProxies)
            {
                if (IPAddress.TryParse(proxy, out var address))
                {
                    options.KnownProxies.Add(address);
                }
            }

            foreach (var network in settings.KnownNetworks)
            {
                if (TryParseNetwork(network, out var parsed))
                {
                    options.KnownNetworks.Add(parsed!);
                }
            }
        });

        return services;
    }

    public static IServiceCollection AddPolyBucketRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        var settings = configuration.GetSection(RateLimitingSettings.SectionName).Get<RateLimitingSettings>()
            ?? new RateLimitingSettings();

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.OnRejected = async (context, cancellationToken) =>
            {
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    context.HttpContext.Response.Headers.RetryAfter = ((int)retryAfter.TotalSeconds).ToString();
                }

                await context.HttpContext.Response.WriteAsJsonAsync(
                    new { message = "Too many requests. Please try again later." },
                    cancellationToken);
            };

            options.AddPolicy(AuthStrictPolicy, httpContext => CreatePartition(httpContext, settings, settings.AuthStrict));
            options.AddPolicy(AuthStandardPolicy, httpContext => CreatePartition(httpContext, settings, settings.AuthStandard));
        });

        return services;
    }

    private static RateLimitPartition<string> CreatePartition(
        HttpContext httpContext,
        RateLimitingSettings settings,
        RateLimitPolicySettings policy)
    {
        if (!settings.Enabled || policy.PermitLimit <= 0)
        {
            return RateLimitPartition.GetNoLimiter("disabled");
        }

        var key = ClientRequestInfo.From(httpContext).IpAddress;
        return RateLimitPartition.GetFixedWindowLimiter(key, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = policy.PermitLimit,
            Window = TimeSpan.FromSeconds(Math.Max(1, policy.WindowSeconds)),
            QueueLimit = 0,
            AutoReplenishment = true
        });
    }

    private static bool TryParseNetwork(string value, out Microsoft.AspNetCore.HttpOverrides.IPNetwork? network)
    {
        network = null;
        var parts = value.Split('/', StringSplitOptions.TrimEntries);
        if (parts.Length != 2 || !IPAddress.TryParse(parts[0], out var prefix) || !int.TryParse(parts[1], out var length))
        {
            return false;
        }

        network = new Microsoft.AspNetCore.HttpOverrides.IPNetwork(prefix, length);
        return true;
    }
}
