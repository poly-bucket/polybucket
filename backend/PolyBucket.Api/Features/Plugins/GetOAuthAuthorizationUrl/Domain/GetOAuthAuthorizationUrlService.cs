using PolyBucket.Api.Features.Plugins.GetOAuthAuthorizationUrl.Repository;

namespace PolyBucket.Api.Features.Plugins.GetOAuthAuthorizationUrl.Domain;

public class GetOAuthAuthorizationUrlService(IGetOAuthAuthorizationUrlRepository repository) : IGetOAuthAuthorizationUrlService
{
    public GetOAuthAuthorizationUrlOutcome GetAuthorizationUrl(string providerName, string redirectUri, string? state)
    {
        if (string.IsNullOrWhiteSpace(providerName))
        {
            return new GetOAuthAuthorizationUrlOutcome { Status = OAuthAuthorizationUrlStatus.BadRequest, ErrorBody = new { message = "Provider name is required" } };
        }

        if (string.IsNullOrWhiteSpace(redirectUri))
        {
            return new GetOAuthAuthorizationUrlOutcome { Status = OAuthAuthorizationUrlStatus.BadRequest, ErrorBody = new { message = "Redirect URI is required" } };
        }

        if (!repository.IsProviderRegistered(providerName))
        {
            return new GetOAuthAuthorizationUrlOutcome { Status = OAuthAuthorizationUrlStatus.NotFound, ErrorBody = new { message = $"OAuth provider '{providerName}' is not registered" } };
        }

        var authUrl = $"https://example.com/oauth/authorize?client_id=example&redirect_uri={Uri.EscapeDataString(redirectUri)}&response_type=code&scope=read&state={state ?? Guid.NewGuid().ToString()}";
        return new GetOAuthAuthorizationUrlOutcome
        {
            Status = OAuthAuthorizationUrlStatus.Ok,
            Response = new OAuthAuthorizationUrlResponse
            {
                AuthorizationUrl = authUrl,
                ProviderName = providerName,
                RedirectUri = redirectUri,
                State = state
            }
        };
    }
}
