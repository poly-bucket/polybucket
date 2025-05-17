using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core.Models;
using Microsoft.EntityFrameworkCore;
using Core.Models.Auth;
using Infrastructure.Data;

namespace Infrastructure.Data.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly DatabaseContext _context;

        public UserRepository(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<User> GetByIdAsync(Guid id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<User> GetByUsernameAsync(string username)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<User> GetByEmailAsync(string email)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User> GetByRefreshTokenAsync(string refreshToken)
        {
            return await _context.Users
                .Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(u => u.RefreshTokens.Any(rt => rt.Token == refreshToken));
        }

        public async Task<IEnumerable<User>> GetAllAsync(int skip = 0, int take = 20)
        {
            return await _context.Users
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        public async Task<User> AddAsync(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<User> UpdateAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var user = await GetByIdAsync(id);
            if (user == null)
                return false;

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UsernameExistsAsync(string username)
        {
            return await _context.Users.AnyAsync(u => u.Username == username);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }

        public async Task<int> GetTotalCountAsync()
        {
            return await _context.Users.CountAsync();
        }

        public async Task<User> CreateAsync(User user, CancellationToken cancellationToken = default)
        {
            await _context.Users.AddAsync(user, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return user;
        }

        public async Task<User> AddRefreshTokenAsync(Guid userId, string token, DateTime expiresAt, CancellationToken cancellationToken = default)
        {
            var user = await _context.Users
                .Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

            if (user == null) return null;

            var refreshToken = new Core.Entities.RefreshToken
            {
                Token = token,
                UserId = userId,
                Created = DateTime.UtcNow,
                Expires = expiresAt,
                CreatedByIp = "127.0.0.1" // This would ideally come from the request
            };

            user.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync(cancellationToken);

            return user;
        }

        public async Task<bool> RevokeRefreshTokenAsync(string token, CancellationToken cancellationToken = default)
        {
            var user = await GetByRefreshTokenAsync(token);
            if (user == null) return false;

            var refreshToken = user.RefreshTokens.FirstOrDefault(t => t.Token == token);
            if (refreshToken == null || !refreshToken.IsActive) return false;

            refreshToken.RevokedByIp = "127.0.0.1"; // This would ideally come from the request
            refreshToken.ReasonRevoked = "Revoked by user";

            await _context.SaveChangesAsync();
            return true;
        }
    }
}