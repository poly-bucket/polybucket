using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using PolyBucket.Api.Features.Comments.ReportComment.Repository;
using PolyBucket.Api.Features.SystemSettings.Services;

namespace PolyBucket.Api.Features.Comments.ReportComment.Domain;

public interface IReportCommentService
{
    Task<bool> ReportAsync(Guid commentId, Guid reporterId, string reason, CancellationToken cancellationToken = default);
}

public class ReportCommentService(IReportCommentRepository repository, ISiteModelAdvancedPolicy advancedPolicy) : IReportCommentService
{
    private readonly IReportCommentRepository _repository = repository;
    private readonly ISiteModelAdvancedPolicy _advancedPolicy = advancedPolicy;

    public async Task<bool> ReportAsync(Guid commentId, Guid reporterId, string reason, CancellationToken cancellationToken = default)
    {
        var advanced = await _advancedPolicy.GetSnapshotAsync(cancellationToken);
        if (!advanced.EnableModelReporting)
        {
            throw new UnauthorizedAccessException("Reporting is disabled on this site.");
        }

        if (advanced.RequireReportDetails && string.IsNullOrWhiteSpace(reason))
        {
            throw new ArgumentException("Report details are required.");
        }

        return await _repository.ReportCommentAsync(commentId, reporterId, reason, cancellationToken);
    }
}
