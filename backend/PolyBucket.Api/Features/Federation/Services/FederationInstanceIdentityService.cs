using Microsoft.AspNetCore.Http;
using PolyBucket.Api.Features.SystemSettings.Services;
using PolyBucket.Email;

namespace PolyBucket.Api.Features.Federation.Services;

public sealed class FederationInstanceIdentityService(
    ISiteFederationPolicy federationPolicy,
    IEmailSettingsResolver emailSettingsResolver,
    IConfiguration configuration,
    IHttpContextAccessor httpContextAccessor) : IFederationInstanceIdentity
{
    private readonly ISiteFederationPolicy _federationPolicy = federationPolicy;
    private readonly IEmailSettingsResolver _emailSettingsResolver = emailSettingsResolver;
    private readonly IConfiguration _configuration = configuration;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public async Task<FederationInstanceIdentitySnapshot> GetSnapshotAsync(CancellationToken cancellationToken = default)
    {
        var policy = await _federationPolicy.GetSnapshotAsync(cancellationToken);
        var baseUrl = await ResolveBaseUrlAsync(cancellationToken);
        var name = policy.HasInstanceName
            ? policy.InstanceName.Trim()
            : "PolyBucket";

        var ready = policy.IsEnabled
                    && policy.HasInstanceName
                    && IsAbsoluteHttpUrl(baseUrl);

        return new FederationInstanceIdentitySnapshot(
            policy.SystemSetupId,
            name,
            policy.InstanceDescription.Trim(),
            policy.AdminContact.Trim(),
            baseUrl,
            ready);
    }

    private async Task<string> ResolveBaseUrlAsync(CancellationToken cancellationToken)
    {
        var emailSettings = await _emailSettingsResolver.GetEffectiveSettingsAsync(cancellationToken);
        if (emailSettings.HasPublicBaseUrl)
        {
            return emailSettings.PublicBaseUrl.TrimEnd('/');
        }

        var configuredUrl = _configuration["Instance:BaseUrl"];
        if (!string.IsNullOrWhiteSpace(configuredUrl))
        {
            return configuredUrl.TrimEnd('/');
        }

        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            return string.Empty;
        }

        var request = httpContext.Request;
        return $"{request.Scheme}://{request.Host}{request.PathBase}".TrimEnd('/');
    }

    private static bool IsAbsoluteHttpUrl(string url)
    {
        return !string.IsNullOrWhiteSpace(url)
               && Uri.TryCreate(url, UriKind.Absolute, out var uri)
               && (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp);
    }
}