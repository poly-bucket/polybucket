using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Data;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Features.Authentication.Domain;
using System;
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

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await _context.Users
                .Include(u => u.Role)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Username == username);
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

        public async Task<PasswordResetToken?> GetPasswordResetTokenAsync(string token)
        {
            return await _context.PasswordResetTokens.FirstOrDefaultAsync(t => t.Token == token);
        }

        public async Task MarkPasswordResetTokenAsUsedAsync(string token)
        {
            var dbToken = await _context.PasswordResetTokens.FirstOrDefaultAsync(t => t.Token == token);
            if (dbToken != null)
            {
                dbToken.IsUsed = true;
                await _context.SaveChangesAsync();
            }
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

        public async Task<EmailVerificationToken?> GetEmailVerificationTokenAsync(string token)
        {
            return await _context.EmailVerificationTokens.FirstOrDefaultAsync(t => t.Token == token);
        }

        public async Task MarkEmailVerificationTokenAsUsedAsync(string token)
        {
            var dbToken = await _context.EmailVerificationTokens.FirstOrDefaultAsync(t => t.Token == token);
            if (dbToken != null)
            {
                dbToken.IsUsed = true;
                await _context.SaveChangesAsync();
            }
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