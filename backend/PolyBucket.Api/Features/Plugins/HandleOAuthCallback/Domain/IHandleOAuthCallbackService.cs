using PolyBucket.Api.Features.Plugins.Domain;
using PolyBucket.Api.Features.Plugins.HandleOAuthCallback.Http;

namespace PolyBucket.Api.Features.Plugins.HandleOAuthCallback.Domain;

public enum HandleOAuthCallbackStatus
{
    Ok,
    BadRequest,
    NotFound
}

public sealed class HandleOAuthCallbackOutcome
{
    public HandleOAuthCallbackStatus Status { get; init; }
    public OAuthAuthorizationResult? Result { get; init; }
    public object? ErrorBody { get; init; }
}

public interface IHandleOAuthCallbackService
{
    Task<HandleOAuthCallbackOutcome> HandleAsync(string providerName, OAuthCallbackRequest request);
}
