using Core.Models.Interfaces;

namespace Core.Models;

public class Model : IEntity
{
    public string Id { get; set; } = null!;
    public string AuthorId { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string License { get; set; } = "CC BY";
    public string Privacy { get; set; } = "Public";
    public List<string> Categories { get; set; } = new();
    public bool AiGenerated { get; set; }
    public bool Wip { get; set; }
    public bool Nsfw { get; set; }
    public bool IsRemix { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public User Author { get; set; } = null!;
    public ICollection<ModelFile> Files { get; set; } = new List<ModelFile>();
    public PrintSettings? PrintSettings { get; set; }
} 