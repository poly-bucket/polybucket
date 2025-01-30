using Core.Models.Users;
using Core.Services;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace Database.Seeders;

public class AdminSeeder
{
    private readonly Context _context;
    private readonly IPasswordHasher _passwordHasher;

    public AdminSeeder(Context context, IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task Seed()
    {
        if (await _context.Users.AnyAsync(u => u.Username == "admin"))
            return;

        var password = GenerateSecurePassword();
        var salt = GenerateSalt();

        var admin = new User
        {
            Username = "admin",
            Email = "admin@localhost",
            FirstName = "frank",
            LastName = "admin",
            Salt = salt,
            PasswordHash = _passwordHasher.HashPassword(password + salt),
            Role = UserRole.SuperAdmin,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        await _context.Users.AddAsync(admin);
        await _context.SaveChangesAsync();

        // Save password to file
        var buildPath = Path.GetDirectoryName(typeof(AdminSeeder).Assembly.Location);
        var passwordFile = Path.Combine(buildPath!, "admin-password.txt");
        File.WriteAllText(passwordFile, $"Admin Password: {password}");
    }

    private string GenerateSecurePassword()
    {
        const string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*()";
        const int passwordLength = 16;

        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[passwordLength];
        rng.GetBytes(bytes);

        var chars = new char[passwordLength];
        for (int i = 0; i < passwordLength; i++)
        {
            chars[i] = validChars[bytes[i] % validChars.Length];
        }

        return new string(chars);
    }

    private string GenerateSalt()
    {
        var bytes = new byte[18]; // 18 bytes will give us 24 characters in base64
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }
}