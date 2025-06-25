using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Users.Domain;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Common.Enums;

namespace PolyBucket.Api.Data.Seeders
{
    public class AdminSeeder
    {
        private readonly PolyBucketDbContext _context;

        public AdminSeeder(PolyBucketDbContext context)
        {
            _context = context;
        }

        public async Task SeedAsync()
        {
            if (await _context.Users.AnyAsync(u => u.Username == "admin"))
            {
                return;
            }

            var salt = BCrypt.Net.BCrypt.GenerateSalt();
            var admin = new User
            {
                Id = Guid.NewGuid(),
                Username = "admin",
                Email = "admin@polybucket.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin", salt),
                Salt = salt,
                Role = UserRole.Admin,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Settings = new UserSettings
                {
                    Id = Guid.NewGuid(),
                    Language = "en",
                    Theme = "dark",
                    EmailNotifications = true,
                    MeasurementSystem = "metric",
                    TimeZone = "UTC",
                    CustomSettings = new Dictionary<string, string>()
                }
            };

            _context.Users.Add(admin);
            await _context.SaveChangesAsync();
        }
    }
} 