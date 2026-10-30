using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using PolyBucket.Api.Features.Comments.Plugins;

namespace PolyBucket.Api.Features.Comments.ReportComment.Repository;

public interface IReportCommentRepository
{
    Task<bool> ReportCommentAsync(Guid commentId, Guid reporterId, string reason, CancellationToken cancellationToken = default);
}

public class ReportCommentRepository(IEnhancedCommentsPlugin plugin) : IReportCommentRepository
{
    public Task<bool> ReportCommentAsync(Guid commentId, Guid reporterId, string reason, CancellationToken cancellationToken = default) =>
        plugin.ReportCommentAsync(commentId, reporterId, reason);
}
