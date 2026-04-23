using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Users.Domain;
using PolyBucket.Api.Features.ACL.Domain;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Features.Authentication.Services;
using Microsoft.Extensions.Logging;
using System.Linq;
using Microsoft.Extensions.Options;
using PolyBucket.Api.Settings;

namespace PolyBucket.Api.Data.Seeders
{
    public class AdminSeeder(
        PolyBucketDbContext context,
        IPasswordGenerator passwordGenerator,
        IPasswordHasher passwordHasher,
        IOptions<AdminSeedSettings> adminSettings,
        ILogger<AdminSeeder> logger)
    {
        private readonly PolyBucketDbContext _context = context;
        private readonly IPasswordGenerator _passwordGenerator = passwordGenerator;
        private readonly IPasswordHasher _passwordHasher = passwordHasher;
        private readonly AdminSeedSettings _adminSettings = adminSettings.Value;
        private readonly ILogger<AdminSeeder> _logger = logger;

        public async Task SeedAsync()
        {
            // Find the Admin role
            var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
            if (adminRole == null)
            {
                throw new InvalidOperationException($"{nameof(AdminSeeder)}: Admin role not found. Please ensure roles are seeded first.");
            }

            // Check if admin users already exist
            var existingAdmins = await _context.Users
                .Where(u => u.RoleId == adminRole.Id)
                .ToListAsync();

            if (existingAdmins.Any())
            {
                _logger.LogInformation($"{nameof(AdminSeeder)}: Admin users already exist, skipping admin seeding");
                return;
            }

            var adminEmail = _adminSettings.Email;
            var adminPassword = string.IsNullOrWhiteSpace(_adminSettings.Password) ? null : _adminSettings.Password;
            var adminUsername = _adminSettings.LoginIdentifier == AdminLoginIdentifier.Email
                ? adminEmail
                : _adminSettings.Username;

            _logger.LogInformation(
                "Admin seeding configuration loaded. LoginIdentifier: {LoginIdentifier}, Username: {Username}, Email: {Email}, PasswordConfigured: {PasswordConfigured}",
                _adminSettings.LoginIdentifier,
                adminUsername,
                adminEmail,
                !string.IsNullOrWhiteSpace(adminPassword));

            await CreateAdminAccount(adminUsername, adminEmail, adminPassword, adminRole);
            _logger.LogInformation($"{nameof(AdminSeeder)}: Admin users created successfully");
        }

        private async Task CreateAdminAccount(string username, string email, string? providedPassword, Role adminRole)
        {
            _logger.LogInformation($"{nameof(AdminSeeder)}: Creating admin user with username: {username}, email: {email}");
            
            // Use provided password or generate a secure random password
            string adminPassword;
            
            if (string.IsNullOrWhiteSpace(providedPassword))
            {
                adminPassword = _passwordGenerator.GeneratePassword(16, 20);
                
                // Log the generated password for the main admin account
                _logger.LogInformation(
                    "Admin account created with generated password. " +
                    "Username: {Username}, Email: {Email}, Password: {Password}. " +
                    "Please change this password after first login. " +
                    "To set a custom password, use the Admin__Password environment variable.",
                    username, email, adminPassword);
            }
            else
            {
                adminPassword = providedPassword;
                _logger.LogInformation("Admin account created with password from configuration. Username: {Username}, Email: {Email}", username, email);
            }

            _logger.LogInformation("Hashing password - Password length: {PasswordLength}", adminPassword.Length);
            
            var salt = _passwordHasher.GenerateSalt();
            _logger.LogInformation("Generated salt: {Salt} (length: {SaltLength})", salt, salt.Length);
            
            var passwordHash = _passwordHasher.HashPassword(adminPassword, salt);
            _logger.LogInformation("Generated password hash: {HashPrefix}... (length: {HashLength})", 
                passwordHash.Substring(0, Math.Min(20, passwordHash.Length)), passwordHash.Length);

            var verifyTest = _passwordHasher.VerifyPassword(adminPassword, passwordHash);
            _logger.LogInformation("Password hash verification test: {VerifyResult}", verifyTest ? "SUCCESS" : "FAILED");
            
            if (!verifyTest)
            {
                _logger.LogError("CRITICAL: Password hash verification failed immediately after creation! This indicates a problem with password hashing.");
            }

            var admin = new User
            {
                Username = username,
                Email = email,
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
            
            _logger.LogInformation($"{nameof(AdminSeeder)}: Admin user created successfully.");
        }
    }
} 