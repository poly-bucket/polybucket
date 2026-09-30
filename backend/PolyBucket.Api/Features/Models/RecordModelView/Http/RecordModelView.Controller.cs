using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Models.Http;
using PolyBucket.Api.Features.Models.Common;
using PolyBucket.Api.Features.Models.RecordModelView.Domain;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Models.RecordModelView.Http;

[ApiController]
[Route("api/models")]
public class RecordModelViewController(
    IRecordModelViewService recordModelViewService,
    IHttpContextAccessor httpContextAccessor) : ControllerBase
{
    /// <summary>
    /// Records a view of a model. Counts at most once per viewer per 24 hours; the model author is not counted.
    /// </summary>
    [HttpPost("{id:guid}/view")]
    [ProducesResponseType(typeof(ModelViewResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RecordModelView(Guid id, CancellationToken cancellationToken)
    {
        var httpContext = httpContextAccessor.HttpContext
            ?? throw new InvalidOperationException("HTTP context is required.");
        var viewerKey = ModelEngagementViewerKey.Build(httpContext, User);
        var outcome = await recordModelViewService.RecordViewAsync(id, User, viewerKey, cancellationToken);

        return outcome.Kind switch
        {
            RecordModelViewOutcomeKind.NotFound => NotFound(),
            RecordModelViewOutcomeKind.Forbid => Forbid(),
            _ => Ok(ModelViewResponse.From(outcome))
        };
    }
}
