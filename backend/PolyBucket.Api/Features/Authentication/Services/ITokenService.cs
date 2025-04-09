using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Features.Authentication.Domain;
using System.Security.Claims;

namespace PolyBucket.Api.Features.Authentication.Services
{
    public interface ITokenService
    {
        string GenerateAccessToken(User user);
        string GenerateRefreshToken();
        ClaimsPrincipal? ValidateAccessToken(string token);
        AuthenticationResponse GenerateAuthenticationResponse(User user);
        string GeneratePasswordResetToken();
        string GenerateEmailVerificationToken();
    }
} 