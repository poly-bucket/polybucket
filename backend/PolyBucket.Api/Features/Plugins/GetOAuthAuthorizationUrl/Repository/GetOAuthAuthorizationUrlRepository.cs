using PolyBucket.Api.Features.Plugins.Services;

namespace PolyBucket.Api.Features.Plugins.GetOAuthAuthorizationUrl.Repository;

public class GetOAuthAuthorizationUrlRepository(OAuthPluginService oauthPluginService) : IGetOAuthAuthorizationUrlRepository
{
    public bool IsProviderRegistered(string providerName) => oauthPluginService.IsProviderRegistered(providerName);
}
