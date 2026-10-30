using Microsoft.EntityFrameworkCore;
using PolyBucket.Database.Entities.Authentication;
using PolyBucket.Database.Entities.Models;
using PolyBucket.Database.Persistence;

namespace PolyBucket.Api.Features.Authentication.VerifyEmail.Repository;

public class VerifyEmailRepository(PolyBucketDbContext context) : IVerifyEmailRepository
{
    private readonly PolyBucketDbContext _context = context;

    public Task<EmailVerificationToken?> GetEmailVerificationTokenByHashAsync(string tokenHash, CancellationToken cancellationToken = default)
    {
        return _context.EmailVerificationTokens
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Token == tokenHash, cancellationToken);
    }

    public Task<User?> GetUserForUpdateByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
    }

    public Task<User?> GetUserForUpdateByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<bool> IsEmailTakenAsync(string email)
    {
        return await _context.Users.AnyAsync(u => u.Email == email);
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

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
