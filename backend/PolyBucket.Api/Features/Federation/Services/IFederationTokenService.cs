using System;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Federation.Services
{
    /// <summary>
    /// Service for generating and validating federation JWT tokens
    /// </summary>
    public interface IFederationTokenService
    {
        /// <summary>
        /// Generate a new federation JWT token
        /// </summary>
        /// <param name="issuerInstanceId">The ID of the instance issuing the token</param>
        /// <param name="audienceInstanceId">The ID of the instance that will receive the token</param>
        /// <param name="issuerUrl">The URL of the issuing instance</param>
        /// <param name="expirationDays">Number of days until token expires (default: 90)</param>
        /// <returns>The generated JWT token string</returns>
        Task<string> GenerateTokenAsync(Guid issuerInstanceId, Guid audienceInstanceId, string issuerUrl, int expirationDays = 90);
        
        /// <summary>
        /// Validate a federation JWT token
        /// </summary>
        /// <param name="token">The JWT token to validate</param>
        /// <param name="expectedAudience">The expected audience (this instance's ID)</param>
        /// <returns>True if token is valid, false otherwise</returns>
        Task<bool> ValidateTokenAsync(string token, Guid expectedAudience);
        
        /// <summary>
        /// Get the expiration date from a token
        /// </summary>
        /// <param name="token">The JWT token</param>
        /// <returns>The expiration date, or null if token is invalid</returns>
        Task<DateTime?> GetTokenExpirationAsync(string token);
        
        /// <summary>
        /// Encrypt a token for storage
        /// </summary>
        /// <param name="token">The token to encrypt</param>
        /// <returns>The encrypted token</returns>
        Task<string> EncryptTokenAsync(string token);
        
        /// <summary>
        /// Decrypt a token from storage
        /// </summary>
        /// <param name="encryptedToken">The encrypted token</param>
        /// <returns>The decrypted token</returns>
        Task<string> DecryptTokenAsync(string encryptedToken);
    }
}

