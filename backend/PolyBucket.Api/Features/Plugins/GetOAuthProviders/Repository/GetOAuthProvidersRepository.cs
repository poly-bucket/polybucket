using PolyBucket.Api.Features.Plugins.Domain;
using PolyBucket.Api.Features.Plugins.Services;

namespace PolyBucket.Api.Features.Plugins.GetOAuthProviders.Repository;

public class GetOAuthProvidersRepository(OAuthPluginService oauthPluginService) : IGetOAuthProvidersRepository
{
    public List<OAuthProviderInfo> GetRegisteredProviders() => oauthPluginService.GetRegisteredProviders();
}
