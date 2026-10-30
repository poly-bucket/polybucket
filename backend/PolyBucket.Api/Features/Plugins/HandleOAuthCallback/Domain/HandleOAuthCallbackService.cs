using PolyBucket.Api.Features.Plugins.HandleOAuthCallback.Http;
using PolyBucket.Api.Features.Plugins.HandleOAuthCallback.Repository;

namespace PolyBucket.Api.Features.Plugins.HandleOAuthCallback.Domain;

public class HandleOAuthCallbackService(IHandleOAuthCallbackRepository repository) : IHandleOAuthCallbackService
{
    public async Task<HandleOAuthCallbackOutcome> HandleAsync(string providerName, OAuthCallbackRequest request)
    {
        if (string.IsNullOrWhiteSpace(providerName))
        {
            return new HandleOAuthCallbackOutcome { Status = HandleOAuthCallbackStatus.BadRequest, ErrorBody = new { message = "Provider name is required" } };
        }

        if (string.IsNullOrWhiteSpace(request.Code))
        {
            return new HandleOAuthCallbackOutcome { Status = HandleOAuthCallbackStatus.BadRequest, ErrorBody = new { message = "Authorization code is required" } };
        }

        if (string.IsNullOrWhiteSpace(request.RedirectUri))
        {
            return new HandleOAuthCallbackOutcome { Status = HandleOAuthCallbackStatus.BadRequest, ErrorBody = new { message = "Redirect URI is required" } };
        }

        if (!repository.IsProviderRegistered(providerName))
        {
            return new HandleOAuthCallbackOutcome { Status = HandleOAuthCallbackStatus.NotFound, ErrorBody = new { message = $"OAuth provider '{providerName}' is not registered" } };
        }

        var result = await repository.AuthorizeAsync(providerName, request.Code, request.RedirectUri);
        if (result.Success)
        {
            return new HandleOAuthCallbackOutcome { Status = HandleOAuthCallbackStatus.Ok, Result = result };
        }

        return new HandleOAuthCallbackOutcome { Status = HandleOAuthCallbackStatus.BadRequest, Result = result };
    }
}
