using PolyBucket.Api.Data;
using PolyBucket.Api.Common.Models;
using System.Threading.Tasks;
using System;
using PolyBucket.Api.Common.Enums;
using System.Collections.Generic;

namespace PolyBucket.Tests.Factories
{
    public class TestUserFactory
    {
        private readonly PolyBucketDbContext _context;

        public TestUserFactory(PolyBucketDbContext context)
        {
            _context = context;
        }

        public async Task<User> CreateAndSaveTestUser(string email = "test@user.com", string password = "password")
        {
            var salt = BCrypt.Net.BCrypt.GenerateSalt();
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = email,
                Username = email.Split('@')[0],
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password, salt),
                Salt = salt,
                Role = UserRole.User,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            return user;
        }
    }
} 