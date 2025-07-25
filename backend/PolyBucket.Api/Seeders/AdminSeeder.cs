using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Users.Domain;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Features.Authentication.Services;
using System.IO;
using Microsoft.Extensions.Logging;

namespace PolyBucket.Api.Data.Seeders
{
    public class AdminSeeder(
        PolyBucketDbContext context,
        IPasswordGenerator passwordGenerator,
        IPasswordHasher passwordHasher,
        ILogger<AdminSeeder> logger)
    {
        private readonly PolyBucketDbContext _context = context;
        private readonly IPasswordGenerator _passwordGenerator = passwordGenerator;
        private readonly IPasswordHasher _passwordHasher = passwordHasher;
        private readonly ILogger<AdminSeeder> _logger = logger;

        public async Task SeedAsync()
        {
            if (await _context.Users.AnyAsync(u => u.Username == "admin"))
            {
                _logger.LogInformation("Admin user already exists, skipping admin seeding");
                return;
            }

            // Find the Admin role
            var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
            if (adminRole == null)
            {
                throw new InvalidOperationException("Admin role not found. Please ensure roles are seeded first.");
            }

            // Generate a secure random password
            var adminPassword = _passwordGenerator.GeneratePassword(16, 20);
            
            // Save password to file in mapped volume
            try
            {
                var passwordFilePath = Path.Combine(Directory.GetCurrentDirectory(), "files", "admin-credentials.txt");
                var credentialsContent = $"PolyBucket Admin Credentials\n" +
                                       $"========================\n" +
                                       $"Username: admin\n" +
                                       $"Email: admin@polybucket.com\n" +
                                       $"Password: {adminPassword}\n" +
                                       $"Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC\n\n" +
                                       $"IMPORTANT: Please change this password after first login for security.\n" +
                                       $"This file should be deleted after you have safely stored the credentials.";
                
                // Ensure the files directory exists
                var filesDirectory = Path.GetDirectoryName(passwordFilePath);
                if (!string.IsNullOrEmpty(filesDirectory) && !Directory.Exists(filesDirectory))
                {
                    Directory.CreateDirectory(filesDirectory);
                }
                
                await File.WriteAllTextAsync(passwordFilePath, credentialsContent);
                _logger.LogInformation("Admin credentials saved to {FilePath}", passwordFilePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save admin credentials to file, but continuing with seeding");
            }

            // Hash the password
            var salt = _passwordHasher.GenerateSalt();
            var passwordHash = _passwordHasher.HashPassword(adminPassword, salt);

            var admin = new User
            {
                Username = "admin",
                Email = "admin@polybucket.com",
                PasswordHash = passwordHash,
                Salt = salt,
                RoleId = adminRole.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                RequiresPasswordChange = true,
                HasCompletedFirstTimeSetup = false,
                Settings = new UserSettings
                {
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
            
            _logger.LogInformation("Admin user created successfully with username: {Username}", admin.Username);
        }
    }
} 