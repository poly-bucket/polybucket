using Core.Models.Interfaces;

namespace Core.Models;

public class User : IEntity
{
    public string Id { get; set; } = null!;
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string Role { get; set; } = "User";
    public string? AvatarUrl { get; set; }
    public string? Bio { get; set; }
    public DateTime JoinDate { get; set; }
    public DateTime LastLoginDate { get; set; }
    public int TotalLikes { get; set; }
    public int TotalDownloads { get; set; }
    public bool IsVerified { get; set; }
    public bool IsBanned { get; set; }
    public UserSettings Settings { get; set; } = null!;
    public ICollection<UserLogin> Logins { get; set; } = new List<UserLogin>();
} 