using Core.Models.Interfaces;

namespace Core.Models;

public class PrintSettings : IEntity
{
    public string Id { get; set; } = null!;
    public string ModelId { get; set; } = null!;
    public float LayerHeight { get; set; }
    public int WallThickness { get; set; }
    public int InfillPercentage { get; set; }
    public bool SupportEnabled { get; set; }
    public string? Notes { get; set; }
    public Model Model { get; set; } = null!;
} 