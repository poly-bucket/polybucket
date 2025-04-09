using Core.Models.Interfaces;

namespace Core.Models;

public class ModelFile : IEntity
{
    public string Id { get; set; } = null!;
    public string ModelId { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Path { get; set; } = null!;
    public long Size { get; set; }
    public string MimeType { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Model Model { get; set; } = null!;
} 