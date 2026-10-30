using PolyBucket.Api.Features.Plugins.Domain;
using PolyBucket.Api.Features.Plugins.Services;

namespace PolyBucket.Api.Features.Plugins.HandleOAuthCallback.Repository;

public class HandleOAuthCallbackRepository(OAuthPluginService oauthPluginService) : IHandleOAuthCallbackRepository
{
    public bool IsProviderRegistered(string providerName) => oauthPluginService.IsProviderRegistered(providerName);

    public Task<OAuthAuthorizationResult> AuthorizeAsync(string providerName, string code, string redirectUri) =>
        oauthPluginService.AuthorizeAsync(providerName, code, redirectUri);
}
