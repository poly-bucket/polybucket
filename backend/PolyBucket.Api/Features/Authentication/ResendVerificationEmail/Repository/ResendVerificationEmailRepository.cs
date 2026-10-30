using Microsoft.EntityFrameworkCore;
using PolyBucket.Database.Entities.Authentication;
using PolyBucket.Database.Entities.Models;
using PolyBucket.Database.Persistence;

namespace PolyBucket.Api.Features.Authentication.ResendVerificationEmail.Repository;

public class ResendVerificationEmailRepository(PolyBucketDbContext context) : IResendVerificationEmailRepository
{
    private readonly PolyBucketDbContext _context = context;

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _context.Users
            .Include(u => u.Role)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public Task<DateTime?> GetLatestEmailVerificationTokenCreatedAtAsync(string email, EmailVerificationPurpose purpose, CancellationToken cancellationToken = default)
    {
        return _context.EmailVerificationTokens
            .Where(t => t.Email == email && t.Purpose == purpose)
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => (DateTime?)t.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<int> InvalidateOutstandingEmailVerificationTokensAsync(string email, EmailVerificationPurpose purpose, DateTime usedAt, CancellationToken cancellationToken = default)
    {
        return _context.EmailVerificationTokens
            .Where(t => t.Email == email && t.Purpose == purpose && !t.IsUsed)
            .ExecuteUpdateAsync(s => s
                .SetProperty(t => t.IsUsed, true)
                .SetProperty(t => t.UsedAt, usedAt), cancellationToken);
    }

    public async Task<EmailVerificationToken> CreateEmailVerificationTokenAsync(EmailVerificationToken token)
    {
        await _context.EmailVerificationTokens.AddAsync(token);
        await _context.SaveChangesAsync();
        return token;
    }
}
