using PolyBucket.Api.Features.Reports.Domain;
using System;

namespace PolyBucket.Api.Features.Reports.SubmitReport.Http
{
    public class SubmitReportRequest
    {
        public ReportType Type { get; set; }
        public Guid TargetId { get; set; }
        public ReportReason Reason { get; set; }
        public required string Description { get; set; }
    }
}
