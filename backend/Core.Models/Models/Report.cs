using Core.Models.Enumerations;
using Core.Models.Interfaces;

namespace Core.Models;

public class Report : IEntity
{
    public string Id { get; set; } = null!;
    public string ReporterId { get; set; } = null!;
    public string TargetId { get; set; } = null!;
    public ReportType Type { get; set; }
    public ReportReason Reason { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsResolved { get; set; }
    public string? ResolutionNotes { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public User Reporter { get; set; } = null!;
} 