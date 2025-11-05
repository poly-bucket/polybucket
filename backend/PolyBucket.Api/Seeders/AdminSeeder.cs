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

            // Read admin configuration from IConfiguration
            // IConfiguration already has the correct priority: Environment Variables > appsettings.{Environment}.json > appsettings.json
            var adminUsername = _configuration["Admin:Username"];
            var adminEmail = _configuration["Admin:Email"];
            var adminPassword = _configuration["Admin:Password"];

            // Log what values are being read from configuration (Information level for visibility in containers)
            _logger.LogInformation(
                "Reading Admin configuration from IConfiguration - Username: {Username}, Email: {Email}, Password: {HasPassword}",
                adminUsername ?? "(null)",
                adminEmail ?? "(null)",
                string.IsNullOrWhiteSpace(adminPassword) ? "(not set - will generate)" : "(provided)");

            // Also log raw environment variables for debugging (if they exist)
            var envUsername = Environment.GetEnvironmentVariable("Admin__Username");
            var envEmail = Environment.GetEnvironmentVariable("Admin__Email");
            var envPassword = Environment.GetEnvironmentVariable("Admin__Password");
            
            if (!string.IsNullOrWhiteSpace(envUsername) || !string.IsNullOrWhiteSpace(envEmail) || !string.IsNullOrWhiteSpace(envPassword))
            {
                _logger.LogInformation(
                    "Environment variables detected - Admin__Username: {EnvUsername}, Admin__Email: {EnvEmail}, Admin__Password: {EnvPasswordSet}",
                    envUsername ?? "(not set)",
                    envEmail ?? "(not set)",
                    string.IsNullOrWhiteSpace(envPassword) ? "(not set)" : "(set)");
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

            // Password is optional - if not provided, will be generated
            if (string.IsNullOrWhiteSpace(adminPassword))
            {
                adminPassword = null;
                _logger.LogInformation("Admin password not configured in IConfiguration, will generate a secure password");
            }
            else
            {
                _logger.LogInformation("Admin password configured from IConfiguration (password value not logged for security)");
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

            // Hash the password
            var salt = _passwordHasher.GenerateSalt();
            var passwordHash = _passwordHasher.HashPassword(adminPassword, salt);

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