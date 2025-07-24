using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Features.Users.Domain;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Users.Repository
{
    public class UserRepository(PolyBucketDbContext context) : IUserRepository
    {
        private readonly PolyBucketDbContext _context = context;

        public async Task<User?> GetByIdAsync(Guid id)
        {
            return await _context.Users
                .Include(u => u.Settings)
                .Include(u => u.Logins)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            return await _context.Users
                .Include(u => u.Settings)
                .Include(u => u.Logins)
                .FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users
                .Include(u => u.Settings)
                .Include(u => u.Logins)
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        // {
        //     return await _context.Users
        //         // .Include(u => u.RefreshTokens)
        //         .FirstOrDefaultAsync(u => u.Logins.Any(rt => rt.Token == refreshToken));
        // }

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

        public async Task<bool> IsEmailTakenAsync(string email, Guid? excludeUserId = null)
        {
            if (excludeUserId == null)
                return await _context.Users.AnyAsync(u => u.Email == email);

            return await _context.Users.AnyAsync(u => u.Email == email && u.Id != excludeUserId);
        }

        public async Task<bool> IsUsernameTakenAsync(string username, Guid? excludeUserId = null)
        {
            if (excludeUserId == null)
                return await _context.Users.AnyAsync(u => u.Username == username);

            return await _context.Users.AnyAsync(u => u.Username == username && u.Id != excludeUserId);
        }

        public async Task<UserProfile> GetUserProfileAsync(Guid id)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                throw new KeyNotFoundException($"User with ID {id} not found");

            return new UserProfile
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                RoleId = user.RoleId,
                RoleName = user.Role?.Name ?? "Unknown",
                CreatedAt = user.CreatedAt
            };
        }

        // ... This needs to be refactored into the new UserLogin model ...
        // public async Task<User> AddRefreshTokenAsync(Guid userId, string token, DateTime expiresAt, CancellationToken cancellationToken = default)
        // {
        //     var user = await _context.Users
        //         .Include(u => u.RefreshTokens)
        //         .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        //     if (user == null) return null;

        //     var refreshToken = new Core.Entities.RefreshToken
        //     {
        //         Token = token,
        //         UserId = userId,
        //         Created = DateTime.UtcNow,
        //         Expires = expiresAt,
        //         CreatedByIp = "127.0.0.1" // This would ideally come from the request
        //     };

        //     user.RefreshTokens.Add(refreshToken);
        //     await _context.SaveChangesAsync(cancellationToken);

        //     return user;
        // }

        // public async Task<bool> RevokeRefreshTokenAsync(string token, CancellationToken cancellationToken = default)
        // {
        //     var user = await GetByRefreshTokenAsync(token);
        //     if (user == null) return false;

        //     var refreshToken = user.RefreshTokens.FirstOrDefault(t => t.Token == token);
        //     if (refreshToken == null || !refreshToken.IsActive) return false;

        //     refreshToken.RevokedByIp = "127.0.0.1"; // This would ideally come from the request
        //     refreshToken.ReasonRevoked = "Revoked by user";

        //     await _context.SaveChangesAsync();
        //     return true;
        // }

        public async Task<PolyBucket.Api.Features.Users.Domain.UserSettings?> GetSettingsByUserIdAsync(Guid userId)
        {
            return await _context.UserSettings.FirstOrDefaultAsync(s => s.UserId == userId);
        }

        public async Task UpdateSettingsAsync(PolyBucket.Api.Features.Users.Domain.UserSettings settings)
        {
            var existingSettings = await _context.UserSettings.AsNoTracking().FirstOrDefaultAsync(s => s.UserId == settings.UserId);
            if (existingSettings != null)
            {
                settings.Id = existingSettings.Id;
                _context.UserSettings.Update(settings);
            }
            else
            {
                _context.UserSettings.Add(settings);
            }
            await _context.SaveChangesAsync();
        }
    }
} 