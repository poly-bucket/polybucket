using Microsoft.EntityFrameworkCore;
using PolyBucket.Database.Entities.Authentication;
using PolyBucket.Database.Entities.Models;
using PolyBucket.Database.Persistence;

namespace PolyBucket.Api.Features.Authentication.ForgotPassword.Repository;

public class ForgotPasswordRepository(PolyBucketDbContext context) : IForgotPasswordRepository
{
    private readonly PolyBucketDbContext _context = context;

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _context.Users
            .Include(u => u.Role)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<PasswordResetToken> CreatePasswordResetTokenAsync(PasswordResetToken token)
    {
        await _context.PasswordResetTokens.AddAsync(token);
        await _context.SaveChangesAsync();
        return token;
    }
}
