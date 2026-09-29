using PolyBucket.Api.Common.Entities;
using PolyBucket.Api.Common.Models;
using System;

namespace PolyBucket.Api.Features.ModelModeration.Domain;

public class ModelModerationRecord : BaseEntity
{
    public Guid ModelId { get; set; }
    public Model Model { get; set; } = null!;
    public ModelModerationStatus Status { get; set; }
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReviewedAt { get; set; }
    public Guid? ReviewedByUserId { get; set; }
    public string? RejectionReason { get; set; }
    public string? Notes { get; set; }
}
