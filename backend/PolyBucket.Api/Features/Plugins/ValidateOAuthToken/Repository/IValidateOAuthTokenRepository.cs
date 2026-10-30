namespace PolyBucket.Api.Features.Plugins.ValidateOAuthToken.Repository;

public interface IValidateOAuthTokenRepository
{
    bool IsProviderRegistered(string providerName);
    Task<bool> ValidateTokenAsync(string providerName, string accessToken);
}
