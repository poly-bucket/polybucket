using System;
using PolyBucket.Database.Entities.Models;

namespace PolyBucket.Database.Entities.Models;

public class ModelDownloadDedup
{
    public Guid Id { get; set; }
    public Guid ModelId { get; set; }
    public Model Model { get; set; } = null!;
    public string ViewerKey { get; set; } = string.Empty;
    public DateTime LastCountedAt { get; set; }
}
