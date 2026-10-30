using PolyBucket.Api.Features.Plugins.Domain;

namespace PolyBucket.Api.Features.Plugins.RefreshOAuthToken.Repository;

public interface IRefreshOAuthTokenRepository
{
    bool IsProviderRegistered(string providerName);
    Task<OAuthTokenRefreshResult> RefreshTokenAsync(string providerName, string refreshToken);
}
