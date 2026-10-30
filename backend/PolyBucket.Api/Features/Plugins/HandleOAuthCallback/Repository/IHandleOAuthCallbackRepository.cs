using PolyBucket.Api.Features.Plugins.Domain;

namespace PolyBucket.Api.Features.Plugins.HandleOAuthCallback.Repository;

public interface IHandleOAuthCallbackRepository
{
    bool IsProviderRegistered(string providerName);

    Task<OAuthAuthorizationResult> AuthorizeAsync(string providerName, string code, string redirectUri);
}
