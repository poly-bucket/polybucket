using PolyBucket.Api.Features.Plugins.Domain;
using PolyBucket.Api.Features.Plugins.GetOAuthUserInfo.Repository;

namespace PolyBucket.Api.Features.Plugins.GetOAuthUserInfo.Domain;

public class GetOAuthUserInfoService(IGetOAuthUserInfoRepository repository) : IGetOAuthUserInfoService
{
    public async Task<OAuthSimpleOutcome<OAuthUserInfo>> GetUserInfoAsync(string providerName, string accessToken)
    {
        if (string.IsNullOrWhiteSpace(providerName))
            return new() { Status = OAuthRequestStatus.BadRequest, ErrorBody = new { message = "Provider name is required" } };
        if (string.IsNullOrWhiteSpace(accessToken))
            return new() { Status = OAuthRequestStatus.BadRequest, ErrorBody = new { message = "Access token is required" } };
        if (!repository.IsProviderRegistered(providerName))
            return new() { Status = OAuthRequestStatus.NotFound, ErrorBody = new { message = $"OAuth provider '{providerName}' is not registered" } };
        return new() { Status = OAuthRequestStatus.Ok, Value = await repository.GetUserInfoAsync(providerName, accessToken) };
    }
}
