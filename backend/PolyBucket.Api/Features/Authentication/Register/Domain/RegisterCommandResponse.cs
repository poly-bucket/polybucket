namespace PolyBucket.Api.Features.Authentication.Register.Domain
{
    public class RegisterCommandResponse
    {
        public RegisterAuthenticationResponse? Authentication { get; set; }
        public bool RequiresEmailVerification { get; set; }
    }
}
