using PolyBucket.Database.Common;
using PolyBucket.Database.Entities.Models;
using System;

namespace PolyBucket.Database.Entities.Reports
{
    public class Report : Auditable
    {
        public ReportType Type { get; set; }
        public Guid TargetId { get; set; }
        public Guid ReporterId { get; set; }
        public ReportReason Reason { get; set; }
        public required string Description { get; set; }
        public bool IsResolved { get; set; }
        public required string Resolution { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public Guid? ResolvedById { get; set; }
    }
}
