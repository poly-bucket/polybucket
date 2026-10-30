using PolyBucket.Api.Features.Plugins.Domain;
using PolyBucket.Api.Features.Plugins.Services;

namespace PolyBucket.Api.Features.Plugins.RefreshOAuthToken.Repository;

public class RefreshOAuthTokenRepository(OAuthPluginService oauthPluginService) : IRefreshOAuthTokenRepository
{
    public bool IsProviderRegistered(string providerName) => oauthPluginService.IsProviderRegistered(providerName);
    public Task<OAuthTokenRefreshResult> RefreshTokenAsync(string providerName, string refreshToken) =>
        oauthPluginService.RefreshTokenAsync(providerName, refreshToken);
}
