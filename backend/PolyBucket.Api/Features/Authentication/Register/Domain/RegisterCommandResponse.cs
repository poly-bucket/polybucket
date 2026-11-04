namespace PolyBucket.Api.Features.Authentication.Register.Domain
{
    using System.Collections.Generic;

    public class RegisterCommandResponse
    {
        public RegisterAuthenticationResponse? Authentication { get; set; }
        public bool RequiresEmailVerification { get; set; }
        public IReadOnlyList<string>? RecoveryCodes { get; set; }
    }
}
