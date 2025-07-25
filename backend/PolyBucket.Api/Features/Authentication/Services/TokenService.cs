using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Features.Authentication.Domain;
using PolyBucket.Api.Features.SystemSettings.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Authentication.Services
{
    public class TokenService(IConfiguration configuration, ITokenSettingsService tokenSettingsService) : ITokenService
    {
        private readonly IConfiguration _configuration = configuration;
        private readonly ITokenSettingsService _tokenSettingsService = tokenSettingsService;

        public async Task<string> GenerateAccessTokenAsync(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user), "User cannot be null");
            }
            
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtSecret = _configuration["AppSettings:Security:JwtSecret"];
            var jwtIssuer = _configuration["AppSettings:Security:JwtIssuer"];
            var jwtAudience = _configuration["AppSettings:Security:JwtAudience"];
            
            if (string.IsNullOrEmpty(jwtSecret))
            {
                throw new InvalidOperationException("JWT Secret is not configured. Please check AppSettings:Security:JwtSecret in configuration.");
            }
            
            if (string.IsNullOrEmpty(jwtIssuer))
            {
                throw new InvalidOperationException("JWT Issuer is not configured. Please check AppSettings:Security:JwtIssuer in configuration.");
            }
            
            if (string.IsNullOrEmpty(jwtAudience))
            {
                throw new InvalidOperationException("JWT Audience is not configured. Please check AppSettings:Security:JwtAudience in configuration.");
            }
            
            var key = Encoding.ASCII.GetBytes(jwtSecret);
            
            // Get token expiry from settings service, fallback to configuration
            var accessTokenExpiryMinutes = await _tokenSettingsService.GetAccessTokenExpiryMinutesAsync();
            if (accessTokenExpiryMinutes <= 0)
            {
                accessTokenExpiryMinutes = Convert.ToInt32(_configuration["AppSettings:Security:AccessTokenExpiryMinutes"] ?? "60");
            }
            
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim(ClaimTypes.Name, user.Username ?? string.Empty),
                new Claim(ClaimTypes.Role, user.Role?.Name ?? "User"),
                new Claim("sub", user.Id.ToString()),
                new Claim("email", user.Email ?? string.Empty),
                new Claim("name", user.Username ?? string.Empty),
                new Claim("role", user.Role?.Name ?? "User"),
                new Claim("email_verified", "true"),
                new Claim("jti", Guid.NewGuid().ToString()),
                new Claim("iat", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            };
            
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(accessTokenExpiryMinutes),
                Issuer = jwtIssuer,
                Audience = jwtAudience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public string GenerateAccessToken(User user)
        {
            // Synchronous wrapper for backward compatibility
            return GenerateAccessTokenAsync(user).GetAwaiter().GetResult();
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
            var jwtSecret = _configuration["AppSettings:Security:JwtSecret"];
            
            if (string.IsNullOrEmpty(jwtSecret))
            {
                return null;
            }
            
            var key = Encoding.ASCII.GetBytes(jwtSecret);

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

        public async Task<AuthenticationResponse> GenerateAuthenticationResponseAsync(User user)
        {
            var accessToken = await GenerateAccessTokenAsync(user);
            var refreshToken = GenerateRefreshToken();
            
            // Get token expiry from settings service, fallback to configuration
            var accessTokenExpiryMinutes = await _tokenSettingsService.GetAccessTokenExpiryMinutesAsync();
            if (accessTokenExpiryMinutes <= 0)
            {
                accessTokenExpiryMinutes = Convert.ToInt32(_configuration["AppSettings:Security:AccessTokenExpiryMinutes"] ?? "60");
            }
            
            var refreshTokenExpiryDays = await _tokenSettingsService.GetRefreshTokenExpiryDaysAsync();
            if (refreshTokenExpiryDays <= 0)
            {
                refreshTokenExpiryDays = Convert.ToInt32(_configuration["AppSettings:Security:RefreshTokenExpiryDays"] ?? "7");
            }
            
            var accessTokenExpiry = DateTime.UtcNow.AddMinutes(accessTokenExpiryMinutes);
            var refreshTokenExpiry = DateTime.UtcNow.AddDays(refreshTokenExpiryDays);

            return new AuthenticationResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                AccessTokenExpiresAt = accessTokenExpiry,
                RefreshTokenExpiresAt = refreshTokenExpiry,
                User = new UserInfo
                {
                    Id = user.Id,
                    Email = user.Email ?? string.Empty,
                    Username = user.Username ?? string.Empty,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Role = user.Role?.Name ?? "User",
                    IsEmailVerified = true, // TODO: Add email verification check
                    CreatedAt = user.CreatedAt,
                    Avatar = user.Avatar
                }
            };
        }

        public AuthenticationResponse GenerateAuthenticationResponse(User user)
        {
            // Synchronous wrapper for backward compatibility
            return GenerateAuthenticationResponseAsync(user).GetAwaiter().GetResult();
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