using PolyBucket.Api.Features.Authentication.Domain;

namespace PolyBucket.Api.Features.Authentication.Register.Domain
{
    public class RegisterCommandResponse
    {
        public AuthenticationResponse Authentication { get; set; } = new();
        public bool RequiresEmailVerification { get; set; }
        public string? EmailVerificationToken { get; set; }
    }
} 