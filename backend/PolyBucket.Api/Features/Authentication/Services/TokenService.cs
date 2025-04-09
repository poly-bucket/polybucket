using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Features.Authentication.Domain;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace PolyBucket.Api.Features.Authentication.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;

        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateAccessToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["AppSettings:Security:JwtSecret"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email),
                    new Claim(JwtRegisteredClaimNames.Name, user.Username),
                    new Claim("role", user.Role.ToString()),
                    new Claim("email_verified", "true") // TODO: Add email verification check
                }),
                Expires = DateTime.UtcNow.AddMinutes(Convert.ToInt32(_configuration["AppSettings:Security:AccessTokenExpiryMinutes"] ?? "60")),
                Issuer = _configuration["AppSettings:Security:JwtIssuer"],
                Audience = _configuration["AppSettings:Security:JwtAudience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        public ClaimsPrincipal? ValidateAccessToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["AppSettings:Security:JwtSecret"]);

            try
            {
                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["AppSettings:Security:JwtIssuer"],
                    ValidateAudience = true,
                    ValidAudience = _configuration["AppSettings:Security:JwtAudience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                return principal;
            }
            catch
            {
                return null;
            }
        }

        public AuthenticationResponse GenerateAuthenticationResponse(User user)
        {
            var accessToken = GenerateAccessToken(user);
            var refreshToken = GenerateRefreshToken();
            var accessTokenExpiry = DateTime.UtcNow.AddMinutes(Convert.ToInt32(_configuration["AppSettings:Security:AccessTokenExpiryMinutes"] ?? "60"));
            var refreshTokenExpiry = DateTime.UtcNow.AddDays(Convert.ToInt32(_configuration["AppSettings:Security:RefreshTokenExpiryDays"] ?? "7"));

            return new AuthenticationResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                AccessTokenExpiresAt = accessTokenExpiry,
                RefreshTokenExpiresAt = refreshTokenExpiry,
                User = new UserInfo
                {
                    Id = user.Id,
                    Email = user.Email,
                    Username = user.Username,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Role = user.Role.ToString(),
                    IsEmailVerified = true, // TODO: Add email verification check
                    CreatedAt = user.CreatedAt
                }
            };
        }

        public string GeneratePasswordResetToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber).Replace("+", "-").Replace("/", "_").Replace("=", "");
        }

        public string GenerateEmailVerificationToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber).Replace("+", "-").Replace("/", "_").Replace("=", "");
        }
    }
} 