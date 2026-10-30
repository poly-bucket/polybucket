using PolyBucket.Api.Features.Plugins.Services;

namespace PolyBucket.Api.Features.Plugins.ValidateOAuthToken.Repository;

public class ValidateOAuthTokenRepository(OAuthPluginService oauthPluginService) : IValidateOAuthTokenRepository
{
    public bool IsProviderRegistered(string providerName) => oauthPluginService.IsProviderRegistered(providerName);
    public Task<bool> ValidateTokenAsync(string providerName, string accessToken) =>
        oauthPluginService.ValidateTokenAsync(providerName, accessToken);
}
