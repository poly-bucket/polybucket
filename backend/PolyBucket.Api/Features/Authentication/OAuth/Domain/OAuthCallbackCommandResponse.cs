using PolyBucket.Api.Features.Authentication.Domain;

namespace PolyBucket.Api.Features.Authentication.OAuth.Domain
{
    public class OAuthCallbackCommandResponse
    {
        public AuthenticationResponse Authentication { get; set; } = new();
        public bool IsNewUser { get; set; }
        public string Provider { get; set; } = string.Empty;
    }
} 