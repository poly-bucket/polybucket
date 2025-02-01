using Core.Models.Interfaces;

namespace Core.Models;

public class UserLogin : IEntity
{
    public string Id { get; set; } = null!;
    public string UserId { get; set; } = null!;
    public string Provider { get; set; } = null!;
    public string ProviderKey { get; set; } = null!;
    public DateTime LoginDate { get; set; }
    public User User { get; set; } = null!;
} 