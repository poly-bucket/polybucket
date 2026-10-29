using PolyBucket.Database.Common;
using PolyBucket.Database.Entities.Models;
using System;

namespace PolyBucket.Database.Entities.ModelModeration
{
    public class ModerationAuditLog : BaseEntity
    {
        public Guid ModelId { get; set; }
        public Guid? PerformedByUserId { get; set; }
        public User? PerformedByUser { get; set; }
        public ModerationAction Action { get; set; }
        public string? PreviousValues { get; set; } // JSON of previous model state
        public string? NewValues { get; set; } // JSON of new model state
        public string? ModerationNotes { get; set; }
        public DateTime PerformedAt { get; set; } = DateTime.UtcNow;
        public string? IPAddress { get; set; }
        public string? UserAgent { get; set; }
    }
} 