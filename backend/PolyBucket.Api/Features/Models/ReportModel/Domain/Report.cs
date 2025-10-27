using PolyBucket.Api.Common.Entities;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Features.Models.Shared.Domain;
using System;

namespace PolyBucket.Api.Features.Models.ReportModel.Domain
{
    public class Report : Auditable
    {
        public new Guid Id { get; set; }
        public Guid ModelId { get; set; }
        public Model Model { get; set; } = null!;
        public Guid ReporterId { get; set; }
        public User Reporter { get; set; } = null!;
        public ReportType Type { get; set; }
        public ReportReason Reason { get; set; }
        public string Description { get; set; } = string.Empty;
        public ReportStatus Status { get; set; }
        public string? AdminNotes { get; set; }
        public Guid? ReviewedById { get; set; }
        public User? ReviewedBy { get; set; }
        public DateTime? ReviewedAt { get; set; }
    }

    public enum ReportType
    {
        Model,
        Comment,
        User
    }

    public enum ReportReason
    {
        Inappropriate,
        Spam,
        Copyright,
        Harassment,
        Violence,
        Other
    }

    public enum ReportStatus
    {
        Pending,
        UnderReview,
        Resolved,
        Dismissed
    }
}
