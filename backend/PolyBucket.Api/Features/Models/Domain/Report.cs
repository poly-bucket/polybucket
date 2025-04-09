using PolyBucket.Api.Common.Entities;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Features.Models.Domain.Enums;
using System;

namespace PolyBucket.Api.Features.Models.Domain
{
    public class Report : Auditable
    {
        public new Guid Id { get; set; }
        public Guid TargetId { get; set; }
        public Guid ReporterId { get; set; }
        public User Reporter { get; set; } = null!;
        public ReportType Type { get; set; }
        public ReportReason Reason { get; set; }
        public string? Description { get; set; }
        public bool IsResolved { get; set; }
        public string? ResolutionNotes { get; set; }
        public DateTime? ResolvedAt { get; set; }
    }
} 