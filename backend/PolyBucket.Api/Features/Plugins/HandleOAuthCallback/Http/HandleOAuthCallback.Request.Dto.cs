namespace PolyBucket.Api.Features.Plugins.HandleOAuthCallback.Http;

public class OAuthCallbackRequest
{
    public string Code { get; set; } = string.Empty;
    public string RedirectUri { get; set; } = string.Empty;
    public string? State { get; set; }
}
