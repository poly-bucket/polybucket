using PolyBucket.Api.Features.Plugins.GetOAuthUserInfo.Domain;
using PolyBucket.Api.Features.Plugins.ValidateOAuthToken.Http;

namespace PolyBucket.Api.Features.Plugins.ValidateOAuthToken.Domain;

public interface IValidateOAuthTokenService
{
    Task<OAuthSimpleOutcome<bool>> ValidateAsync(string providerName, OAuthValidateRequest request);
}
