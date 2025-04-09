using Core.Models.Users;
using Core.Models.Users.Settings;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace Database.Seeders;

public class AdminSeeder
{
    private readonly Context _context;

    public AdminSeeder(Context context)
    {
        _context = context;
    }

    public async Task SeedAsync()
    {
        var adminId = Guid.NewGuid();
        if (await _context.Users.FindAsync(adminId) != null)
            return;

        var admin = new User
        {
            Id = adminId,
            Username = "admin",
            Email = "admin@polybucket.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin"),
            Salt = BCrypt.Net.BCrypt.GenerateSalt(),
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