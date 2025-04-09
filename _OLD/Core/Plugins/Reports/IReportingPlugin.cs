using System.ComponentModel.Composition;

namespace Core.Plugins.Reports
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

    /// <summary>
    /// Interface that must be implemented by reporting plugins
    /// </summary>
    public interface IReportingPlugin : IPlugin
    {
        /// <summary>
        /// Submit a new report
        /// </summary>
        Task<Report> SubmitReportAsync(ReportType type, Guid targetId, Guid reporterId, ReportReason reason, string description);

        /// <summary>
        /// Get all reports for a specific target
        /// </summary>
        Task<IEnumerable<Report>> GetReportsForTargetAsync(ReportType type, Guid targetId);

        /// <summary>
        /// Get all unresolved reports
        /// </summary>
        Task<IEnumerable<Report>> GetUnresolvedReportsAsync();

        /// <summary>
        /// Resolve a report
        /// </summary>
        Task ResolveReportAsync(Guid reportId, Guid resolverId, string resolution);

        /// <summary>
        /// Get report by ID
        /// </summary>
        Task<Report> GetReportByIdAsync(Guid reportId);
    }
} 