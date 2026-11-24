using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Features.Models.Upload;
using PolyBucket.Database.Entities.Models;
using PolyBucket.Database.Entities.Users;
using PolyBucket.Database.Persistence;

namespace PolyBucket.Api.Seeders;

public class AnonymousUploadAuthorSeeder(PolyBucketDbContext context, ILogger<AnonymousUploadAuthorSeeder> logger)
{
    private readonly PolyBucketDbContext _context = context;
    private readonly ILogger<AnonymousUploadAuthorSeeder> _logger = logger;

    public async Task SeedAsync()
    {
        var exists = await _context.Users.AnyAsync(u => u.Id == AnonymousUploadAuthor.UserId);
        if (exists)
        {
            return;
        }

        var userRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "User");
        if (userRole == null)
        {
            throw new InvalidOperationException($"{nameof(AnonymousUploadAuthorSeeder)}: User role not found.");
        }

        var now = DateTime.UtcNow;
        var user = new User
        {
            Id = AnonymousUploadAuthor.UserId,
            Email = AnonymousUploadAuthor.Email,
            Username = AnonymousUploadAuthor.Username,
            FirstName = "Anonymous",
            LastName = "Uploader",
            Salt = string.Empty,
            PasswordHash = string.Empty,
            RoleId = userRole.Id,
            CanLogin = false,
            IsProfilePublic = false,
            CreatedAt = now,
            UpdatedAt = now,
            CreatedById = AnonymousUploadAuthor.UserId,
            UpdatedById = AnonymousUploadAuthor.UserId,
            Settings = new UserSettings
            {
                Id = Guid.NewGuid(),
                UserId = AnonymousUploadAuthor.UserId,
                Language = "en",
                Theme = "dark",
                EmailNotifications = false,
                MeasurementSystem = "metric",
                TimeZone = "UTC",
                CustomSettings = new Dictionary<string, string>()
            }
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Anonymous upload author user seeded with id {UserId}", AnonymousUploadAuthor.UserId);
    }
}