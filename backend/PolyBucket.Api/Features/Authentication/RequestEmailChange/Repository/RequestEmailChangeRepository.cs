using Microsoft.EntityFrameworkCore;
using PolyBucket.Database.Entities.Authentication;
using PolyBucket.Database.Entities.Models;
using PolyBucket.Database.Persistence;

namespace PolyBucket.Api.Features.Authentication.RequestEmailChange.Repository;

public class RequestEmailChangeRepository(PolyBucketDbContext context) : IRequestEmailChangeRepository
{
    private readonly PolyBucketDbContext _context = context;

    public Task<User?> GetUserForUpdateByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
    }

    public async Task<bool> IsEmailTakenAsync(string email)
    {
        return await _context.Users.AnyAsync(u => u.Email == email);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
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
