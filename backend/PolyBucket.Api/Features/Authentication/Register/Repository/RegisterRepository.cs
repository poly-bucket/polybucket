using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PolyBucket.Database.Entities.Authentication;
using PolyBucket.Database.Entities.Models;
using PolyBucket.Database.Persistence;
using RefreshTokenModel = PolyBucket.Database.Entities.Authentication.RefreshToken;

namespace PolyBucket.Api.Features.Authentication.Register.Repository;

public class RegisterRepository(PolyBucketDbContext context, ILogger<RegisterRepository> logger) : IRegisterRepository
{
    private readonly PolyBucketDbContext _context = context;
    private readonly ILogger<RegisterRepository> _logger = logger;

    public async Task<bool> IsEmailTakenAsync(string email)
    {
        return await _context.Users.AnyAsync(u => u.Email == email);
    }

    public async Task<bool> IsUsernameTakenAsync(string username)
    {
        return await _context.Users.AnyAsync(u => u.Username == username);
    }

    public async Task<Guid?> GetDefaultUserRoleIdAsync(CancellationToken cancellationToken)
    {
        var systemSetup = await _context.SystemSetups.AsNoTracking().FirstOrDefaultAsync(cancellationToken);
        var roleName = string.IsNullOrWhiteSpace(systemSetup?.DefaultUserRole) ? "User" : systemSetup.DefaultUserRole.Trim();

        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == roleName, cancellationToken);
        if (role != null)
        {
            return role.Id;
        }

        if (!string.Equals(roleName, "User", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Default user role {RoleName} was not found; falling back to User", roleName);
        }

        role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "User", cancellationToken);
        return role?.Id;
    }

    public async Task<User> CreateUserAsync(User user)
    {
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<EmailVerificationToken> CreateEmailVerificationTokenAsync(EmailVerificationToken token)
    {
        await _context.EmailVerificationTokens.AddAsync(token);
        await _context.SaveChangesAsync();
        return token;
    }

    public async Task<RefreshTokenModel> CreateRefreshTokenAsync(RefreshTokenModel refreshToken)
    {
        await _context.RefreshTokens.AddAsync(refreshToken);
        await _context.SaveChangesAsync();
        return refreshToken;
    }

    public async Task CreateLoginRecordAsync(UserLogin userLogin)
    {
        await _context.UserLogins.AddAsync(userLogin);
        await _context.SaveChangesAsync();
    }
}
