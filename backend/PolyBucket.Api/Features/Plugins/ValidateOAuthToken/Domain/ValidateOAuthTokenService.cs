using PolyBucket.Api.Features.Plugins.GetOAuthUserInfo.Domain;
using PolyBucket.Api.Features.Plugins.ValidateOAuthToken.Http;
using PolyBucket.Api.Features.Plugins.ValidateOAuthToken.Repository;

namespace PolyBucket.Api.Features.Plugins.ValidateOAuthToken.Domain;

public class ValidateOAuthTokenService(IValidateOAuthTokenRepository repository) : IValidateOAuthTokenService
{
    public async Task<OAuthSimpleOutcome<bool>> ValidateAsync(string providerName, OAuthValidateRequest request)
    {
        if (string.IsNullOrWhiteSpace(providerName))
            return new() { Status = OAuthRequestStatus.BadRequest, ErrorBody = new { message = "Provider name is required" } };
        if (string.IsNullOrWhiteSpace(request.AccessToken))
            return new() { Status = OAuthRequestStatus.BadRequest, ErrorBody = new { message = "Access token is required" } };
        if (!repository.IsProviderRegistered(providerName))
            return new() { Status = OAuthRequestStatus.NotFound, ErrorBody = new { message = $"OAuth provider '{providerName}' is not registered" } };
        return new() { Status = OAuthRequestStatus.Ok, Value = await repository.ValidateTokenAsync(providerName, request.AccessToken) };
    }
}
