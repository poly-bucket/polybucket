using PolyBucket.Api.Features.Plugins.Services;
using PolyBucket.Api.Features.Plugins.GetOAuthProviders.Repository;

namespace PolyBucket.Api.Features.Plugins.GetOAuthProviders.Domain;

public class GetOAuthProvidersService(IGetOAuthProvidersRepository repository) : IGetOAuthProvidersService
{
    public List<OAuthProviderInfo> GetProviders() => repository.GetRegisteredProviders();
}
