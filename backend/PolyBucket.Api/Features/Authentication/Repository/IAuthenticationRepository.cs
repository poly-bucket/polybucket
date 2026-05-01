using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Features.Authentication.Domain;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RefreshTokenModel = PolyBucket.Api.Features.Authentication.Domain.RefreshToken;

namespace PolyBucket.Api.Features.Authentication.Repository
{
    public interface IAuthenticationRepository
    {
        Task<User?> GetUserByEmailAsync(string email);
        Task<User?> GetUserByUsernameAsync(string username);
        Task<User> CreateUserAsync(User user);
        Task CreateLoginRecordAsync(UserLogin userLogin);
        Task<bool> IsEmailTakenAsync(string email);
        Task<bool> IsUsernameTakenAsync(string username);
        
        // Refresh Token methods
        Task<RefreshTokenModel> CreateRefreshTokenAsync(RefreshTokenModel refreshToken);
        Task<RefreshTokenModel?> GetRefreshTokenAsync(string token);
        Task RevokeRefreshTokenAsync(string token, string reason, string revokedByIp);
        Task<RefreshTokenModel?> GetActiveRefreshTokenByIdAsync(Guid tokenId, Guid userId);
        Task<IReadOnlyList<RefreshTokenModel>> GetActiveRefreshTokensForUserAsync(Guid userId);
        Task RevokeRefreshTokenByIdAsync(Guid tokenId, string reason, string revokedByIp);
        Task RevokeAllRefreshTokensForUserAsync(Guid userId, string reason, string revokedByIp);
        
        // Password Reset methods
        Task<PasswordResetToken> CreatePasswordResetTokenAsync(PasswordResetToken token);
        Task<PasswordResetToken?> GetPasswordResetTokenAsync(string token);
        Task MarkPasswordResetTokenAsUsedAsync(string token);
        Task DeleteExpiredPasswordResetTokensAsync();
        
        // Email Verification methods
        Task<EmailVerificationToken> CreateEmailVerificationTokenAsync(EmailVerificationToken token);
        Task<EmailVerificationToken?> GetEmailVerificationTokenAsync(string token);
        Task MarkEmailVerificationTokenAsUsedAsync(string token);
        Task DeleteExpiredEmailVerificationTokensAsync();
        
        // OAuth methods
        Task<ExternalAuthProvider?> GetExternalAuthProviderAsync(string provider, string externalId);
        Task<ExternalAuthProvider> CreateExternalAuthProviderAsync(ExternalAuthProvider provider);
        Task UpdateExternalAuthProviderAsync(ExternalAuthProvider provider);
        Task<ExternalAuthProvider?> GetExternalAuthProviderByEmailAsync(string provider, string email);
    }
} 