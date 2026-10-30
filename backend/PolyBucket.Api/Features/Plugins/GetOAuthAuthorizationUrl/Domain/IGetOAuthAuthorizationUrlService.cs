namespace PolyBucket.Api.Features.Plugins.GetOAuthAuthorizationUrl.Domain;

public sealed class OAuthAuthorizationUrlResponse
{
    public string AuthorizationUrl { get; init; } = string.Empty;
    public string ProviderName { get; init; } = string.Empty;
    public string RedirectUri { get; init; } = string.Empty;
    public string? State { get; init; }
}

public enum OAuthAuthorizationUrlStatus
{
    Ok,
    BadRequest,
    NotFound
}

public sealed class GetOAuthAuthorizationUrlOutcome
{
    public OAuthAuthorizationUrlStatus Status { get; init; }
    public OAuthAuthorizationUrlResponse? Response { get; init; }
    public object? ErrorBody { get; init; }
}

public interface IGetOAuthAuthorizationUrlService
{
    GetOAuthAuthorizationUrlOutcome GetAuthorizationUrl(string providerName, string redirectUri, string? state);
}
