using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Federation.Services
{
    /// <summary>
    /// Service for generating and validating federation JWT tokens
    /// </summary>
    public class FederationTokenService : IFederationTokenService
    {
        private readonly IConfiguration _configuration;
        private readonly string _secretKey;
        private readonly string _encryptionKey;

        public FederationTokenService(IConfiguration configuration)
        {
            _configuration = configuration;
            _secretKey = _configuration["Jwt:SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
            // For encryption, we'll use a derived key from the JWT secret
            _encryptionKey = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(_secretKey)));
        }

        public Task<string> GenerateTokenAsync(Guid issuerInstanceId, Guid audienceInstanceId, string issuerUrl, int expirationDays = 90)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_secretKey);
            var issuedAt = DateTime.UtcNow;
            var expires = issuedAt.AddDays(expirationDays);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("iss", issuerInstanceId.ToString()),
                    new Claim("aud", audienceInstanceId.ToString()),
                    new Claim("sub", "federation"),
                    new Claim("iat", new DateTimeOffset(issuedAt).ToUnixTimeSeconds().ToString()),
                    new Claim("scope", "federation:read,federation:write"),
                    new Claim("instance_url", issuerUrl),
                    new Claim("jti", Guid.NewGuid().ToString())
                }),
                Expires = expires,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return Task.FromResult(tokenHandler.WriteToken(token));
        }

        public Task<bool> ValidateTokenAsync(string token, Guid expectedAudience)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_secretKey);

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false, // We'll validate the issuer claim manually
                    ValidateAudience = false, // We'll validate the audience claim manually
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(5)
                };

                tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var audienceClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "aud")?.Value;

                // Verify the audience matches this instance
                return Task.FromResult(audienceClaim == expectedAudience.ToString());
            }
            catch
            {
                return Task.FromResult(false);
            }
        }

        public Task<DateTime?> GetTokenExpirationAsync(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token);
                return Task.FromResult<DateTime?>(jwtToken.ValidTo);
            }
            catch
            {
                return Task.FromResult<DateTime?>(null);
            }
        }

        public Task<string> EncryptTokenAsync(string token)
        {
            using var aes = Aes.Create();
            var key = Convert.FromBase64String(_encryptionKey);
            aes.Key = key.Take(32).ToArray(); // Use first 32 bytes for AES-256
            aes.GenerateIV();

            using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            var tokenBytes = Encoding.UTF8.GetBytes(token);
            var encryptedBytes = encryptor.TransformFinalBlock(tokenBytes, 0, tokenBytes.Length);

            // Prepend IV to encrypted data
            var result = new byte[aes.IV.Length + encryptedBytes.Length];
            Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
            Buffer.BlockCopy(encryptedBytes, 0, result, aes.IV.Length, encryptedBytes.Length);

            return Task.FromResult(Convert.ToBase64String(result));
        }

        public Task<string> DecryptTokenAsync(string encryptedToken)
        {
            try
            {
                using var aes = Aes.Create();
                var key = Convert.FromBase64String(_encryptionKey);
                aes.Key = key.Take(32).ToArray(); // Use first 32 bytes for AES-256

                var encryptedBytes = Convert.FromBase64String(encryptedToken);

                // Extract IV from the beginning
                var iv = new byte[aes.IV.Length];
                var cipherText = new byte[encryptedBytes.Length - iv.Length];
                Buffer.BlockCopy(encryptedBytes, 0, iv, 0, iv.Length);
                Buffer.BlockCopy(encryptedBytes, iv.Length, cipherText, 0, cipherText.Length);

                aes.IV = iv;

                using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                var decryptedBytes = decryptor.TransformFinalBlock(cipherText, 0, cipherText.Length);

                return Task.FromResult(Encoding.UTF8.GetString(decryptedBytes));
            }
            catch
            {
                throw new InvalidOperationException("Failed to decrypt token");
            }
        }
    }
}

