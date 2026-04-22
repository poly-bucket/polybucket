using PolyBucket.Api.Common.Entities;
using PolyBucket.Api.Common.Models;
using System;

namespace PolyBucket.Api.Features.Reports.Domain
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
