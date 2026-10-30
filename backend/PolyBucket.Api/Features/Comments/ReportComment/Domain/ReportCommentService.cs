using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using PolyBucket.Api.Features.Comments.ReportComment.Repository;

namespace PolyBucket.Api.Features.Comments.ReportComment.Domain;

public interface IReportCommentService
{
    Task<bool> ReportAsync(Guid commentId, Guid reporterId, string reason, CancellationToken cancellationToken = default);
}

public class ReportCommentService(IReportCommentRepository repository) : IReportCommentService
{
    public Task<bool> ReportAsync(Guid commentId, Guid reporterId, string reason, CancellationToken cancellationToken = default) =>
        repository.ReportCommentAsync(commentId, reporterId, reason, cancellationToken);
}
