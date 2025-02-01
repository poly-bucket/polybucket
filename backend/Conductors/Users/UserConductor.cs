using Core.Models.Users;
using Core.Models.Users.Settings;
using Database;
using Microsoft.EntityFrameworkCore;

namespace Conductors.Users;

public interface IUserConductor
{
    Task<User?> GetUserByIdAsync(string id);

    Task<User?> GetUserByEmailAsync(string email);

    Task<User?> GetUserByUsernameAsync(string username);

    Task<bool> IsEmailTakenAsync(string email, string? excludeUserId = null);

    Task<bool> IsUsernameTakenAsync(string username, string? excludeUserId = null);

    Task<UserProfile> GetUserProfileAsync(Guid id);
}

public class UserConductor : IUserConductor
{
    private readonly Context _context;

    public UserConductor(Context context)
    {
        _context = context;
    }

    public async Task<User?> GetUserByIdAsync(string id)
    {
        if (!Guid.TryParse(id, out Guid guidId))
            return null;

        return await _context.Users
            .Include(u => u.Settings)
            .Include(u => u.Logins)
            .FirstOrDefaultAsync(u => u.Id == guidId);
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _context.Users
            .Include(u => u.Settings)
            .Include(u => u.Logins)
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        return await _context.Users
            .Include(u => u.Settings)
            .Include(u => u.Logins)
            .FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task<bool> IsEmailTakenAsync(string email, string? excludeUserId = null)
    {
        if (excludeUserId == null)
            return await _context.Users.AnyAsync(u => u.Email == email);

        if (!Guid.TryParse(excludeUserId, out Guid excludeGuidId))
            return await _context.Users.AnyAsync(u => u.Email == email);

        return await _context.Users.AnyAsync(u => u.Email == email && u.Id != excludeGuidId);
    }

    public async Task<bool> IsUsernameTakenAsync(string username, string? excludeUserId = null)
    {
        if (excludeUserId == null)
            return await _context.Users.AnyAsync(u => u.Username == username);

        if (!Guid.TryParse(excludeUserId, out Guid excludeGuidId))
            return await _context.Users.AnyAsync(u => u.Username == username);

        return await _context.Users.AnyAsync(u => u.Username == username && u.Id != excludeGuidId);
    }

    public async Task<UserProfile> GetUserProfileAsync(Guid id)
    {
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
            throw new KeyNotFoundException($"User with ID {id} not found");

        return new UserProfile
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role,
            CreatedAt = user.CreatedAt
        };
    }
}