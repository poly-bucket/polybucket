using PolyBucket.Api.Features.Authentication.Domain;

namespace PolyBucket.Api.Features.Authentication.RefreshToken.Domain
{
    public class RefreshTokenCommandResponse
    {
        public AuthenticationResponse Authentication { get; set; } = new();
    }
} 