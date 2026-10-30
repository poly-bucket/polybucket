using PolyBucket.Api.Features.Plugins.Services;
using PolyBucket.Api.Features.Plugins.Domain;

namespace PolyBucket.Api.Features.Plugins.GetOAuthProviders.Domain;

public interface IGetOAuthProvidersService
{
    List<OAuthProviderInfo> GetProviders();
}
