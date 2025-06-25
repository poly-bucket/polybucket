using PolyBucket.Api.Common.Plugins;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Reports.Domain
{
    public enum ReportType
    {
        Model,
        Comment
    }

    public enum ReportReason
    {
        Inappropriate,
        Spam,
        Copyright,
        Other
    }

    public class Report
    {
        public Guid Id { get; set; }
        public ReportType Type { get; set; }
        public Guid TargetId { get; set; }
        public Guid ReporterId { get; set; }
        public ReportReason Reason { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsResolved { get; set; }
        public string Resolution { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public Guid? ResolvedById { get; set; }
    }

    public interface IReportingPlugin : IPlugin
    {
        Task<Report> SubmitReportAsync(ReportType type, Guid targetId, Guid reporterId, ReportReason reason, string description);
        Task<IEnumerable<Report>> GetReportsForTargetAsync(ReportType type, Guid targetId);
        Task<IEnumerable<Report>> GetUnresolvedReportsAsync();
        Task ResolveReportAsync(Guid reportId, Guid resolverId, string resolution);
        Task<Report> GetReportByIdAsync(Guid reportId);
    }
} 