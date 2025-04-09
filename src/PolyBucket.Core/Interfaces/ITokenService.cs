using PolyBucket.Core.Entities;
using System;
using System.Security.Claims;

namespace PolyBucket.Core.Interfaces
{
    public interface ITokenService
    {
        string GenerateAccessToken(User user);
        string GenerateRefreshToken();
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
    }
} 