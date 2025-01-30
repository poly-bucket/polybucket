using Database;
using Microsoft.EntityFrameworkCore;
using Core.Models.Users;

namespace Conductors.Users;

public interface IUserConductor
{
    Task<User?> GetUserByIdAsync(Guid id);

    Task<UserProfile> GetUserProfileAsync(Guid id);
}

public class UserConductor : IUserConductor
{
    private readonly Context _context;

    public UserConductor(Context context)
    {
        _context = context;
    }

    public async Task<User?> GetUserByIdAsync(Guid id)
    {
        return await _context.Users.FindAsync(id);
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