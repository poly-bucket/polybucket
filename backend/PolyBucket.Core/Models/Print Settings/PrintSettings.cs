using Core.Entities;
using Core.Models.Interfaces;
using Core.Models.Models;

namespace Core.Models;

public class PrintSettings : Auditable
{
    public Guid ModelId { get; set; }
    public float LayerHeight { get; set; }
    public int WallThickness { get; set; }
    public int InfillPercentage { get; set; }
    public bool SupportEnabled { get; set; }
    public string? Notes { get; set; }
    public Model Model { get; set; } = null!;
}