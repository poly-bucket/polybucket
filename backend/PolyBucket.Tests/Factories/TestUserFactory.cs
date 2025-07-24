using PolyBucket.Api.Data;
using PolyBucket.Api.Common.Models;
using System.Threading.Tasks;
using System;
using PolyBucket.Api.Features.ACL.Domain;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Features.Users.Domain;

namespace PolyBucket.Tests.Factories
{
    public class TestUserFactory(PolyBucketDbContext context)
    {
        private readonly PolyBucketDbContext _context = context;
        private static int _userCounter = 0;

        public async Task<User> CreateTestUser(string email = null, string password = "TestPassword123!")
        {
            _userCounter++;
            var uniqueEmail = email ?? $"testuser{_userCounter}@example.com";
            
            // Check if user already exists
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == uniqueEmail);
            if (existingUser != null)
            {
                return existingUser;
            }

            var salt = BCrypt.Net.BCrypt.GenerateSalt();
            
            // Get or create the User role
            var userRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "User");
            if (userRole == null)
            {
                userRole = new Role
                {
                    Id = Guid.NewGuid(),
                    Name = "User",
                    Description = "Standard user role",
                    IsActive = true,
                    IsDefault = true,
                    IsSystemRole = true,
                    CanBeDeleted = false,
                    Priority = 100
                };
                _context.Roles.Add(userRole);
                await _context.SaveChangesAsync();
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = uniqueEmail,
                Username = $"testuser{_userCounter}",
                FirstName = "Test",
                LastName = $"User{_userCounter}",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password, salt),
                Salt = salt,
                RoleId = userRole.Id,
                IsBanned = false,
                HasCompletedFirstTimeSetup = true,
                RequiresPasswordChange = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            
            return user;
        }

        public async Task<User> CreateAndSaveTestUser(string email = null, string password = "TestPassword123!")
        {
            return await CreateTestUser(email, password);
        }
    }
} 