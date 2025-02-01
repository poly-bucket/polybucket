using Core.Models.Interfaces;

namespace Core.Models;

public class Comment : IEntity
{
    public string Id { get; set; } = null!;
    public string TargetId { get; set; } = null!;
    public string AuthorId { get; set; } = null!;
    public string Content { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsEdited { get; set; }
    public bool IsDeleted { get; set; }
    public User Author { get; set; } = null!;
} 