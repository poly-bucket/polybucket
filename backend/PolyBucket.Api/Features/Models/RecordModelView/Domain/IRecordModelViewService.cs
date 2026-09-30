using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Models.RecordModelView.Domain;

public interface IRecordModelViewService
{
    Task<RecordModelViewOutcome> RecordViewAsync(
        Guid modelId,
        ClaimsPrincipal user,
        string viewerKey,
        CancellationToken cancellationToken = default);
}
