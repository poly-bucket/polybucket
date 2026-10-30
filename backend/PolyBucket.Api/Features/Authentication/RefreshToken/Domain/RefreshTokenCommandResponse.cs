namespace PolyBucket.Api.Features.Authentication.RefreshToken.Domain;

public class RefreshTokenCommandResponse
{
    public RefreshTokenAuthenticationResponse Authentication { get; set; } = new();
}
