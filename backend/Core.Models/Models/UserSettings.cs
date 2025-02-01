using Core.Models.Interfaces;

namespace Core.Models;

public class UserSettings : IEntity
{
    public string Id { get; set; } = null!;
    public string UserId { get; set; } = null!;
    public string Theme { get; set; } = "light";
    public string Language { get; set; } = "en";
    public bool EmailNotifications { get; set; } = true;
    public bool PushNotifications { get; set; } = true;
    public User User { get; set; } = null!;
} 