using PolyBucket.Api.Features.Plugins.Domain;
using PolyBucket.Api.Features.Plugins.GetOAuthUserInfo.Domain;
using PolyBucket.Api.Features.Plugins.RefreshOAuthToken.Http;
using PolyBucket.Api.Features.Plugins.RefreshOAuthToken.Repository;

namespace PolyBucket.Api.Features.Plugins.RefreshOAuthToken.Domain;

public class RefreshOAuthTokenService(IRefreshOAuthTokenRepository repository) : IRefreshOAuthTokenService
{
    public async Task<OAuthSimpleOutcome<OAuthTokenRefreshResult>> RefreshAsync(string providerName, OAuthRefreshRequest request)
    {
        if (string.IsNullOrWhiteSpace(providerName))
            return new() { Status = OAuthRequestStatus.BadRequest, ErrorBody = new { message = "Provider name is required" } };
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            return new() { Status = OAuthRequestStatus.BadRequest, ErrorBody = new { message = "Refresh token is required" } };
        if (!repository.IsProviderRegistered(providerName))
            return new() { Status = OAuthRequestStatus.NotFound, ErrorBody = new { message = $"OAuth provider '{providerName}' is not registered" } };
        var result = await repository.RefreshTokenAsync(providerName, request.RefreshToken);
        return result.Success
            ? new() { Status = OAuthRequestStatus.Ok, Value = result }
            : new() { Status = OAuthRequestStatus.BadRequest, Value = result };
    }
}
