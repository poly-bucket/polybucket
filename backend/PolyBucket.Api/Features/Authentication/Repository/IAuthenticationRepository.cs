using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Features.Authentication.Domain;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RefreshTokenModel = PolyBucket.Api.Features.Authentication.Domain.RefreshToken;

namespace PolyBucket.Api.Features.Authentication.Repository
{
    public interface IAuthenticationRepository
    {
        Task<User?> GetUserByEmailAsync(string email);
        Task<User?> GetUserForUpdateByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<User?> GetUserForUpdateByIdAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<User?> GetUserByUsernameAsync(string username);
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
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
        Task<PasswordResetToken?> GetPasswordResetTokenByHashAsync(string tokenHash, CancellationToken cancellationToken = default);
        Task<bool> TryConsumePasswordResetTokenAsync(Guid tokenId, DateTime usedAt, CancellationToken cancellationToken = default);
        Task<int> InvalidateOutstandingPasswordResetTokensAsync(string email, DateTime usedAt, CancellationToken cancellationToken = default);
        Task DeleteExpiredPasswordResetTokensAsync();
        
        // Email Verification methods
        Task<EmailVerificationToken> CreateEmailVerificationTokenAsync(EmailVerificationToken token);
        Task<EmailVerificationToken?> GetEmailVerificationTokenByHashAsync(string tokenHash, CancellationToken cancellationToken = default);
        Task<bool> TryConsumeEmailVerificationTokenAsync(Guid tokenId, DateTime usedAt, CancellationToken cancellationToken = default);
        Task<int> InvalidateOutstandingEmailVerificationTokensAsync(string email, EmailVerificationPurpose purpose, DateTime usedAt, CancellationToken cancellationToken = default);
        Task<DateTime?> GetLatestEmailVerificationTokenCreatedAtAsync(string email, EmailVerificationPurpose purpose, CancellationToken cancellationToken = default);
        Task DeleteExpiredEmailVerificationTokensAsync();
        
        // OAuth methods
        Task<ExternalAuthProvider?> GetExternalAuthProviderAsync(string provider, string externalId);
        Task<ExternalAuthProvider> CreateExternalAuthProviderAsync(ExternalAuthProvider provider);
        Task UpdateExternalAuthProviderAsync(ExternalAuthProvider provider);
        Task<ExternalAuthProvider?> GetExternalAuthProviderByEmailAsync(string provider, string email);
    }
} 