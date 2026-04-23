using System.ComponentModel.DataAnnotations;

namespace PolyBucket.Api.Settings;

public sealed class AdminSeedSettings
{
    public const string SectionName = "Admin";

    [Required]
    public string Username { get; set; } = "admin";

    [Required]
    [EmailAddress]
    public string Email { get; set; } = "admin@polybucket.com";

    public string? Password { get; set; }

    public AdminLoginIdentifier LoginIdentifier { get; set; } = AdminLoginIdentifier.Username;
}

public enum AdminLoginIdentifier
{
    Username,
    Email
}
