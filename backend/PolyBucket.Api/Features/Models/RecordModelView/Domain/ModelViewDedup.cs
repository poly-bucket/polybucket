using System;
using PolyBucket.Api.Common.Models;

namespace PolyBucket.Api.Features.Models.RecordModelView.Domain;

public class ModelViewDedup
{
    public Guid Id { get; set; }
    public Guid ModelId { get; set; }
    public Model Model { get; set; } = null!;
    public string ViewerKey { get; set; } = string.Empty;
    public DateTime LastCountedAt { get; set; }
}
