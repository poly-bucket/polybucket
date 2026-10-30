using PolyBucket.Api.Features.Plugins.Domain;

namespace PolyBucket.Api.Features.Plugins.GetOAuthUserInfo.Repository;

public interface IGetOAuthUserInfoRepository
{
    bool IsProviderRegistered(string providerName);
    Task<OAuthUserInfo> GetUserInfoAsync(string providerName, string accessToken);
}
