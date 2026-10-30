using PolyBucket.Api.Features.Plugins.Services;
using PolyBucket.Api.Features.Plugins.Domain;

namespace PolyBucket.Api.Features.Plugins.GetOAuthProviders.Repository;

public interface IGetOAuthProvidersRepository
{
    List<OAuthProviderInfo> GetRegisteredProviders();
}
