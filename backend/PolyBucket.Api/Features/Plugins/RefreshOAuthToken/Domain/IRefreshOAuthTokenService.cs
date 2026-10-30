using PolyBucket.Api.Features.Plugins.Domain;
using PolyBucket.Api.Features.Plugins.GetOAuthUserInfo.Domain;
using PolyBucket.Api.Features.Plugins.RefreshOAuthToken.Http;

namespace PolyBucket.Api.Features.Plugins.RefreshOAuthToken.Domain;

public interface IRefreshOAuthTokenService
{
    Task<OAuthSimpleOutcome<OAuthTokenRefreshResult>> RefreshAsync(string providerName, OAuthRefreshRequest request);
}
