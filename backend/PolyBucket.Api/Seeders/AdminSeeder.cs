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
using Microsoft.Extensions.Configuration;

namespace PolyBucket.Api.Data.Seeders
{
    public class AdminSeeder(
        PolyBucketDbContext context,
        IPasswordGenerator passwordGenerator,
        IPasswordHasher passwordHasher,
        IConfiguration configuration,
        ILogger<AdminSeeder> logger)
    {
        private readonly PolyBucketDbContext _context = context;
        private readonly IPasswordGenerator _passwordGenerator = passwordGenerator;
        private readonly IPasswordHasher _passwordHasher = passwordHasher;
        private readonly IConfiguration _configuration = configuration;
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

            _logger.LogInformation("=== AdminSeeder: Starting configuration read ===");
            
            var envUsername = Environment.GetEnvironmentVariable("Admin__Username");
            var envEmail = Environment.GetEnvironmentVariable("Admin__Email");
            var envPassword = Environment.GetEnvironmentVariable("Admin__Password");
            
            _logger.LogInformation(
                "Raw environment variables - Admin__Username: {EnvUsername}, Admin__Email: {EnvEmail}, Admin__Password: {EnvPasswordInfo}",
                envUsername ?? "(not set)",
                envEmail ?? "(not set)",
                string.IsNullOrWhiteSpace(envPassword) ? "(not set)" : $"(set, length: {envPassword.Length})");

            var configUsername = _configuration["Admin:Username"];
            var configEmail = _configuration["Admin:Email"];
            var configPassword = _configuration["Admin:Password"];
            
            _logger.LogInformation(
                "IConfiguration values - Admin:Username: {ConfigUsername}, Admin:Email: {ConfigEmail}, Admin:Password: {ConfigPasswordInfo}",
                configUsername ?? "(null)",
                configEmail ?? "(null)",
                string.IsNullOrWhiteSpace(configPassword) ? "(null)" : $"(set, length: {configPassword.Length})");

            // Prioritize environment variables over configuration (environment variables take precedence)
            var adminUsername = envUsername ?? configUsername;
            var adminEmail = envEmail ?? configEmail;
            var adminPassword = envPassword ?? configPassword;

            if (!string.IsNullOrWhiteSpace(envUsername) && envUsername != configUsername)
            {
                _logger.LogWarning("Mismatch: Environment variable Admin__Username='{Env}' but IConfiguration Admin:Username='{Config}'", envUsername, configUsername);
            }
            if (!string.IsNullOrWhiteSpace(envEmail) && envEmail != configEmail)
            {
                _logger.LogWarning("Mismatch: Environment variable Admin__Email='{Env}' but IConfiguration Admin:Email='{Config}'", envEmail, configEmail);
            }
            if (!string.IsNullOrWhiteSpace(envPassword) && envPassword != configPassword)
            {
                _logger.LogWarning("Mismatch: Environment variable Admin__Password length={EnvLen} but IConfiguration Admin:Password length={ConfigLen}", 
                    envPassword.Length, configPassword?.Length ?? 0);
            }

            // Use defaults if not provided
            if (string.IsNullOrWhiteSpace(adminUsername))
            {
                adminUsername = "admin";
                _logger.LogInformation("Admin username not configured in IConfiguration, using default: {Username}", adminUsername);
            }
            else
            {
                _logger.LogInformation("Admin username configured: {Username}", adminUsername);
            }

            if (string.IsNullOrWhiteSpace(adminEmail))
            {
                adminEmail = "admin@polybucket.com";
                _logger.LogInformation("Admin email not configured in IConfiguration, using default: {Email}", adminEmail);
            }
            else
            {
                _logger.LogInformation("Admin email configured: {Email}", adminEmail);
            }

            if (string.IsNullOrWhiteSpace(adminPassword))
            {
                adminPassword = null;
                _logger.LogInformation("Admin password not configured in IConfiguration, will generate a secure password");
            }
            else
            {
                var maskedPassword = adminPassword.Length > 2 
                    ? $"{adminPassword[0]}{new string('*', adminPassword.Length - 2)}{adminPassword[adminPassword.Length - 1]}"
                    : "**";
                _logger.LogInformation(
                    "Admin password configured from IConfiguration - Length: {Length}, Masked: {MaskedPassword}, First char: '{FirstChar}', Last char: '{LastChar}'",
                    adminPassword.Length, maskedPassword, adminPassword[0], adminPassword[adminPassword.Length - 1]);
            }

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