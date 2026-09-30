using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Data;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Features.Authentication.Domain;
using System;
using System.Threading;
using System.Threading.Tasks;
using RefreshTokenModel = PolyBucket.Api.Features.Authentication.Domain.RefreshToken;

namespace PolyBucket.Api.Features.Authentication.Repository
{
    public class AuthenticationRepository(PolyBucketDbContext context) : IAuthenticationRepository
    {
        private readonly PolyBucketDbContext _context = context;

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _context.Users
                .Include(u => u.Role)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public Task<User?> GetUserForUpdateByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
        }

        public Task<User?> GetUserForUpdateByIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await _context.Users
                .Include(u => u.Role)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Username == username);
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<User> CreateUserAsync(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task CreateLoginRecordAsync(UserLogin userLogin)
        {
            await _context.UserLogins.AddAsync(userLogin);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> IsEmailTakenAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }

        public async Task<bool> IsUsernameTakenAsync(string username)
        {
            return await _context.Users.AnyAsync(u => u.Username == username);
        }

        public async Task<RefreshTokenModel> CreateRefreshTokenAsync(RefreshTokenModel refreshToken)
        {
            await _context.RefreshTokens.AddAsync(refreshToken);
            await _context.SaveChangesAsync();
            return refreshToken;
        }

        public async Task<RefreshTokenModel?> GetRefreshTokenAsync(string token)
        {
            return await _context.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == token);
        }

        public async Task RevokeRefreshTokenAsync(string token, string reason, string revokedByIp)
        {
            var refreshToken = await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == token);
            if (refreshToken != null)
            {
                refreshToken.RevokedAt = DateTime.UtcNow;
                refreshToken.ReasonRevoked = reason;
                refreshToken.RevokedByIp = revokedByIp;
                await _context.SaveChangesAsync();
            }
        }

        public Task<RefreshTokenModel?> GetActiveRefreshTokenByIdAsync(Guid tokenId, Guid userId)
        {
            return _context.RefreshTokens
                .Where(rt => rt.Id == tokenId && rt.UserId == userId && rt.RevokedAt == null && rt.ExpiresAt > DateTime.UtcNow)
                .FirstOrDefaultAsync();
        }

        public async Task<IReadOnlyList<RefreshTokenModel>> GetActiveRefreshTokensForUserAsync(Guid userId)
        {
            return await _context.RefreshTokens
                .Where(rt => rt.UserId == userId && rt.RevokedAt == null && rt.ExpiresAt > DateTime.UtcNow)
                .OrderByDescending(rt => rt.CreatedAt)
                .ToListAsync();
        }

        public async Task RevokeRefreshTokenByIdAsync(Guid tokenId, string reason, string revokedByIp)
        {
            var refreshToken = await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Id == tokenId);
            if (refreshToken == null || refreshToken.RevokedAt != null)
            {
                return;
            }

            refreshToken.RevokedAt = DateTime.UtcNow;
            refreshToken.ReasonRevoked = reason;
            refreshToken.RevokedByIp = revokedByIp;
            await _context.SaveChangesAsync();
        }

        public async Task RevokeAllRefreshTokensForUserAsync(Guid userId, string reason, string revokedByIp)
        {
            var userRefreshTokens = await _context.RefreshTokens.Where(rt => rt.UserId == userId && rt.RevokedAt == null).ToListAsync();
            foreach (var token in userRefreshTokens)
            {
                token.RevokedAt = DateTime.UtcNow;
                token.ReasonRevoked = reason;
                token.RevokedByIp = revokedByIp;
            }
            await _context.SaveChangesAsync();
        }

        public async Task<PasswordResetToken> CreatePasswordResetTokenAsync(PasswordResetToken token)
        {
            await _context.PasswordResetTokens.AddAsync(token);
            await _context.SaveChangesAsync();
            return token;
        }

        public Task<PasswordResetToken?> GetPasswordResetTokenByHashAsync(string tokenHash, CancellationToken cancellationToken = default)
        {
            return _context.PasswordResetTokens
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Token == tokenHash, cancellationToken);
        }

        public async Task<bool> TryConsumePasswordResetTokenAsync(Guid tokenId, DateTime usedAt, CancellationToken cancellationToken = default)
        {
            var updated = await _context.PasswordResetTokens
                .Where(t => t.Id == tokenId && !t.IsUsed && t.ExpiresAt > usedAt)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(t => t.IsUsed, true)
                    .SetProperty(t => t.UsedAt, usedAt), cancellationToken);
            return updated == 1;
        }

        public Task<int> InvalidateOutstandingPasswordResetTokensAsync(string email, DateTime usedAt, CancellationToken cancellationToken = default)
        {
            return _context.PasswordResetTokens
                .Where(t => t.Email == email && !t.IsUsed)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(t => t.IsUsed, true)
                    .SetProperty(t => t.UsedAt, usedAt), cancellationToken);
        }

        public async Task DeleteExpiredPasswordResetTokensAsync()
        {
            var expiredTokens = await _context.PasswordResetTokens
                .Where(t => t.ExpiresAt < DateTime.UtcNow || t.IsUsed)
                .ToListAsync();
            
            _context.PasswordResetTokens.RemoveRange(expiredTokens);
            await _context.SaveChangesAsync();
        }

        public async Task<EmailVerificationToken> CreateEmailVerificationTokenAsync(EmailVerificationToken token)
        {
            await _context.EmailVerificationTokens.AddAsync(token);
            await _context.SaveChangesAsync();
            return token;
        }

        public Task<EmailVerificationToken?> GetEmailVerificationTokenByHashAsync(string tokenHash, CancellationToken cancellationToken = default)
        {
            return _context.EmailVerificationTokens
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Token == tokenHash, cancellationToken);
        }

        public async Task<bool> TryConsumeEmailVerificationTokenAsync(Guid tokenId, DateTime usedAt, CancellationToken cancellationToken = default)
        {
            var updated = await _context.EmailVerificationTokens
                .Where(t => t.Id == tokenId && !t.IsUsed && t.ExpiresAt > usedAt)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(t => t.IsUsed, true)
                    .SetProperty(t => t.UsedAt, usedAt), cancellationToken);
            return updated == 1;
        }

        public Task<int> InvalidateOutstandingEmailVerificationTokensAsync(string email, EmailVerificationPurpose purpose, DateTime usedAt, CancellationToken cancellationToken = default)
        {
            return _context.EmailVerificationTokens
                .Where(t => t.Email == email && t.Purpose == purpose && !t.IsUsed)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(t => t.IsUsed, true)
                    .SetProperty(t => t.UsedAt, usedAt), cancellationToken);
        }

        public Task<DateTime?> GetLatestEmailVerificationTokenCreatedAtAsync(string email, EmailVerificationPurpose purpose, CancellationToken cancellationToken = default)
        {
            return _context.EmailVerificationTokens
                .Where(t => t.Email == email && t.Purpose == purpose)
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => (DateTime?)t.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task DeleteExpiredEmailVerificationTokensAsync()
        {
            var expiredTokens = await _context.EmailVerificationTokens
                .Where(t => t.ExpiresAt < DateTime.UtcNow || t.IsUsed)
                .ToListAsync();

            _context.EmailVerificationTokens.RemoveRange(expiredTokens);
            await _context.SaveChangesAsync();
        }

        public async Task<ExternalAuthProvider?> GetExternalAuthProviderAsync(string provider, string externalId)
        {
            return await _context.ExternalAuthProviders
                .FirstOrDefaultAsync(p => p.Provider == provider && p.ExternalId == externalId);
        }

        public async Task<ExternalAuthProvider> CreateExternalAuthProviderAsync(ExternalAuthProvider provider)
        {
            await _context.ExternalAuthProviders.AddAsync(provider);
            await _context.SaveChangesAsync();
            return provider;
        }

        public async Task UpdateExternalAuthProviderAsync(ExternalAuthProvider provider)
        {
            _context.ExternalAuthProviders.Update(provider);
            await _context.SaveChangesAsync();
        }

        public async Task<ExternalAuthProvider?> GetExternalAuthProviderByEmailAsync(string provider, string email)
        {
            return await _context.ExternalAuthProviders
                .FirstOrDefaultAsync(p => p.Provider == provider && p.User.Email == email);
        }
    }
}
