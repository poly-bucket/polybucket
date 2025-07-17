using PolyBucket.Api.Common.Entities;
using PolyBucket.Api.Common.Models;
using System;

namespace PolyBucket.Api.Features.ModelModeration.Domain
{
    public class ModerationAuditLog : BaseEntity
    {
        public Guid ModelId { get; set; }
        public Guid PerformedByUserId { get; set; }
        public User PerformedByUser { get; set; } = null!;
        public ModerationAction Action { get; set; }
        public string? PreviousValues { get; set; } // JSON of previous model state
        public string? NewValues { get; set; } // JSON of new model state
        public string? ModerationNotes { get; set; }
        public DateTime PerformedAt { get; set; } = DateTime.UtcNow;
        public string? IPAddress { get; set; }
        public string? UserAgent { get; set; }
    }
} 