using Core.Models.Users;
using Core.Services;
using Database;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;

namespace Tests.Factories;

public class TestUserFactory(IPasswordHasher passwordHasher, Context context)
{
    private readonly IPasswordHasher _passwordHasher = passwordHasher;
    private readonly Context _context = context;

    public User CreateTestUser(string email = "admin@localhost.local", string password = "Password123!")
    {
        var salt = GenerateSalt();
        var hashedPassword = _passwordHasher.HashPassword(password + salt);

        return new User
        {
            Email = email,
            FirstName = "Test",
            LastName = "User",
            Role = UserRole.SuperAdmin,
            Country = "US",
            Username = "testuser",
            PasswordHash = hashedPassword,
            Salt = salt,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public async Task<User> CreateAndSaveTestUser(
        string email = "test@localhost.local",
        string firstName = "test",
        string lastName = "user",
        UserRole role = UserRole.User,
        string country = "US",
        string username = "test_user",
        string password = "password")
    {
        var salt = GenerateSalt();
        var hashedPassword = _passwordHasher.HashPassword(password + salt);

        var testUser = new User
        {
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            Role = role,
            Country = country,
            Username = username,
            PasswordHash = hashedPassword,
            Salt = salt,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _context.Users.AddAsync(testUser);

        await _context.SaveChangesAsync();

        return _context.Users.First(u => u.Email == email);
    }

    private string GenerateSalt()
    {
        var bytes = new byte[18]; // 18 bytes will give us 24 characters in base64
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }
}