using PolyBucket.Api.Features.Plugins.Domain;
using PolyBucket.Api.Features.Plugins.Services;

namespace PolyBucket.Api.Features.Plugins.GetOAuthUserInfo.Repository;

public class GetOAuthUserInfoRepository(OAuthPluginService oauthPluginService) : IGetOAuthUserInfoRepository
{
    public bool IsProviderRegistered(string providerName) => oauthPluginService.IsProviderRegistered(providerName);
    public Task<OAuthUserInfo> GetUserInfoAsync(string providerName, string accessToken) =>
        oauthPluginService.GetUserInfoAsync(providerName, accessToken);
}
